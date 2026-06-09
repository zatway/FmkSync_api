using Microsoft.EntityFrameworkCore.Migrations;
using static Infrastructure.Persistence.Seed.DemoSeedIds;

namespace Infrastructure.Persistence.Seed;

internal static class DemoDataSeeder
{
    private const string Ts = "2026-06-01 00:00:00+00";

    public static void Up(MigrationBuilder migrationBuilder)
    {
        SeedDepartments(migrationBuilder);
        SeedPositions(migrationBuilder);
        SeedUsers(migrationBuilder);
        SeedProjects(migrationBuilder);
        SeedProjectMembers(migrationBuilder);
        SeedStatusColumns(migrationBuilder);
        SeedTags(migrationBuilder);
        SeedTasks(migrationBuilder);
        SeedProjectComments(migrationBuilder);
        SeedTaskComments(migrationBuilder);
        SeedKnowledgeArticles(migrationBuilder);
    }

    public static void Down(MigrationBuilder migrationBuilder)
    {
        var userIds = string.Join(", ", Users.All.Select(id => $"'{id}'"));
        var projectIds = string.Join(", ", Projects.All.Select(id => $"'{id}'"));
        var deptIds = string.Join(", ", new[]
        {
            Departments.Implementation, Departments.Integration, Departments.Service,
            Departments.Documentation, Departments.Analytics, Departments.RnD,
            Departments.DeploymentSupport, Departments.CrmCompetence, Departments.Commercial
        }.Select(id => $"'{id}'"));
        var posIds = string.Join(", ", new[]
        {
            Positions.ProjectLead, Positions.ImplementationSpecialist, Positions.IntegrationEngineer,
            Positions.Analyst, Positions.DevLead, Positions.ImplementationEngineer2, Positions.Analyst1C,
            Positions.VibroEngineer, Positions.SensorRnD, Positions.ServiceEngineer,
            Positions.SalesManager, Positions.ScadaDeveloper, Positions.TechnicalWriter
        }.Select(id => $"'{id}'"));

        migrationBuilder.Sql($"""
            DELETE FROM "TaskComments" WHERE "TaskId" IN (SELECT "Id" FROM "Tasks" WHERE "ProjectId" IN ({projectIds}));
            DELETE FROM "ProjectComments" WHERE "ProjectId" IN ({projectIds});
            DELETE FROM "ProjectTaskTags" WHERE "ProjectTaskId" IN (SELECT "Id" FROM "Tasks" WHERE "ProjectId" IN ({projectIds}));
            DELETE FROM "Tasks" WHERE "ProjectId" IN ({projectIds});
            DELETE FROM "KnowledgeArticles" WHERE "AuthorId" IN ({userIds});
            DELETE FROM "ProjectTaskStatusColumns" WHERE "ProjectId" IN ({projectIds});
            DELETE FROM "Tags" WHERE "ProjectId" IN ({projectIds});
            DELETE FROM "ProjectUser" WHERE "ProjectsId" IN ({projectIds});
            DELETE FROM "Projects" WHERE "Id" IN ({projectIds});
            DELETE FROM "Users" WHERE "Id" IN ({userIds});
            DELETE FROM "Positions" WHERE "Id" IN ({posIds});
            DELETE FROM "Departments" WHERE "Id" IN ({deptIds});
            """);
    }

    private static void SeedDepartments(MigrationBuilder mb) => mb.Sql($"""
        INSERT INTO "Departments" ("Id","Name","Description","CreatedAt") VALUES
        ('{Departments.Implementation}','Отдел внедрения и диагностики','Пилотные внедрения, вибромониторинг, ПНР','{Ts}'),
        ('{Departments.Integration}','Отдел автоматизации АСУ ТП','Интеграции, SCADA, телемеханика, REST API','{Ts}'),
        ('{Departments.Service}','Сервис и сопровождение','Горячая линия, релизы, выездной сервис','{Ts}'),
        ('{Departments.Documentation}','Документация и методология','База знаний, регламенты, обучение','{Ts}'),
        ('{Departments.Analytics}','Аналитическая служба','ТЗ, обработка диагностических данных','{Ts}'),
        ('{Departments.RnD}','НИОКР и разработка датчиков','MEMS, акселерометры, ИВК, контроллеры','{Ts}'),
        ('{Departments.DeploymentSupport}','Отдел сопровождения внедрений','Гарантия, обучение, вторая линия','{Ts}'),
        ('{Departments.CrmCompetence}','Центр компетенций CRM','Методология, типовые настройки','{Ts}'),
        ('{Departments.Commercial}','Коммерческий отдел','Тендеры, заказчики ТЭК','{Ts}')
        ON CONFLICT ("Id") DO NOTHING;
        """);

    private static void SeedPositions(MigrationBuilder mb) => mb.Sql($"""
        INSERT INTO "Positions" ("Id","Name","Description","DepartmentId","CreatedAt") VALUES
        ('{Positions.ProjectLead}','Руководитель проектов','Управление внедрениями и координация','{Departments.Implementation}','{Ts}'),
        ('{Positions.ImplementationSpecialist}','Специалист по внедрению','Настройка систем, обучение','{Departments.Implementation}','{Ts}'),
        ('{Positions.IntegrationEngineer}','Инженер интеграций','API, очереди, АСУ ТП','{Departments.Integration}','{Ts}'),
        ('{Positions.Analyst}','Аналитик','Снятие требований, ТЗ','{Departments.Analytics}','{Ts}'),
        ('{Positions.DevLead}','Тимлид разработки ПО','Координация разработки ИВК','{Departments.RnD}','{Ts}'),
        ('{Positions.ImplementationEngineer2}','Инженер внедрения II кат.','Полевые работы до 30 %','{Departments.Implementation}','{Ts}'),
        ('{Positions.Analyst1C}','Аналитик 1С','Снятие требований, ТЗ','{Departments.Integration}','{Ts}'),
        ('{Positions.VibroEngineer}','Инженер вибродиагностики','Обследование динамического оборудования','{Departments.Implementation}','{Ts}'),
        ('{Positions.SensorRnD}','Инженер НИОКР датчиков','Разработка акселерометров MEMS/ICP','{Departments.RnD}','{Ts}'),
        ('{Positions.ServiceEngineer}','Сервисный инженер','Гарантийное и постгарантийное обслуживание','{Departments.Service}','{Ts}'),
        ('{Positions.SalesManager}','Менеджер проектов','Работа с заказчиками ТЭК','{Departments.Commercial}','{Ts}'),
        ('{Positions.ScadaDeveloper}','Разработчик ПО АСУ','ИВК, контроллеры, нейросетевые модули','{Departments.RnD}','{Ts}'),
        ('{Positions.TechnicalWriter}','Технический писатель','Эксплуатационная документация','{Departments.Documentation}','{Ts}')
        ON CONFLICT ("Id") DO NOTHING;
        """);

    private static void SeedUsers(MigrationBuilder mb) => mb.Sql($"""
        INSERT INTO "Users" ("Id","FullName","Email","NormalizedEmail","PasswordHash","Role","IsApproved","PositionId","DepartmentId","CreatedAt") VALUES
        ('{Users.DemoAdmin}','Администратор Демо','demo@komsync.local','DEMO@KOMSYNC.LOCAL','$2a$13$F65F/XSTp/XKfLRP0g.yxuPKCsX9srTRQCPO1.KGXQko/66Zc3KDO',3,true,'{Positions.ProjectLead}','{Departments.Implementation}','{Ts}'),
        ('{Users.Kozlov}','Козлов Артём Сергеевич','kozlov.as@mail.ru','KOZLOV.AS@MAIL.RU','$2a$13$iEMhkAoFWZGDaUELbA0bmOL/TGlo2XczYkK4RxLnmSZ5TTCs4RrUC',2,true,'{Positions.ProjectLead}','{Departments.Implementation}','{Ts}'),
        ('{Users.Lebedeva}','Лебедева Марина Игоревна','lebedeva.mi@corp.local','LEBEDEVA.MI@CORP.LOCAL','$2a$13$K61uj9t1dgLNpQhyfQqIaeJ4mbRn3DcYuvBDkb1xrCoJlQ/EgTNki',1,true,'{Positions.ImplementationSpecialist}','{Departments.Implementation}','{Ts}'),
        ('{Users.Petrov}','Петров Илья Ильич','petrov.ii@mail.ru','PETROV.II@MAIL.RU','$2a$13$BPKOPfB1QfHhw8gAIeNJLOaeyl1eVQthk5IlKV1FYtc5MgRMxTTnC',1,true,'{Positions.IntegrationEngineer}','{Departments.Documentation}','{Ts}'),
        ('{Users.Orlova}','Орлова Ксения Павловна','orlova.kp@corp.local','ORLOVA.KP@CORP.LOCAL','$2a$13$bsZ9SKI3Ur/Q.d1k.9NtAOXVH7pOMC54d/CMa4QStFX7z3Unf7U0.',0,true,'{Positions.Analyst}','{Departments.Analytics}','{Ts}'),
        ('{Users.Volkov}','Волков Никита Сергеевич','volkov.ns@mail.ru','VOLKOV.NS@MAIL.RU','$2a$13$s6Bdt2HmjYyI4LqWvja9ougZJWtDQzpwuRaMJq5wu1eRRSn130obG',2,true,'{Positions.DevLead}','{Departments.RnD}','{Ts}'),
        ('{Users.Smirnov}','Смирнов Денис Алексеевич','smirnov.da@comdiag.ru','SMIRNOV.DA@COMDIAG.RU','$2a$13$pu4y3bhZIvrK3jguLh2gbejv5EKhuHfkdkLLfXjXSbgDfQJ/TWcHW',1,true,'{Positions.VibroEngineer}','{Departments.Implementation}','{Ts}'),
        ('{Users.Kravtsova}','Кравцова Елена Викторовна','kravtsova.ev@comdiag.ru','KRAVTSOVA.EV@COMDIAG.RU','$2a$13$ufAtTDeruosRHnkWGIrBzeYZv6b5DgNVECdRchjmJE.V1FoUGa1X2',1,true,'{Positions.Analyst}','{Departments.Analytics}','{Ts}'),
        ('{Users.Danilov}','Данилов Роман Игоревич','danilov.ri@comdiag.ru','DANILOV.RI@COMDIAG.RU','$2a$13$ws7BBddfW.Nbm/yPrvyBHO4NMfz9NgKx/LvZgDLJZ5W.ZpUH5l4da',1,true,'{Positions.ServiceEngineer}','{Departments.Service}','{Ts}'),
        ('{Users.Zhukov}','Жуков Павел Михайлович','zhukov.pm@comdiag.ru','ZHUKOV.PM@COMDIAG.RU','$2a$13$86hlU7qFUwPfOuuEpQ8ssu0y.8McA0255GcSQjiuICdE3SpieI..e',2,true,'{Positions.SalesManager}','{Departments.Commercial}','{Ts}'),
        ('{Users.Medvedeva}','Медведева Анна Олеговна','medvedeva.ao@comdiag.ru','MEDVEDEVA.AO@COMDIAG.RU','$2a$13$WCKeO7Oc0I4LoY/wMWaZXOhMXaL3FV8TJpxUrpYlrwn3biOhuWJBm',1,true,'{Positions.SensorRnD}','{Departments.RnD}','{Ts}'),
        ('{Users.Sokolov}','Соколов Игорь Геннадьевич','sokolov.ig@comdiag.ru','SOKOLOV.IG@COMDIAG.RU','$2a$13$jyWqf5mJhgzOXzRpj.aNiemlBJkmVibaR7/lxGHPhmL60BER.oMsy',1,true,'{Positions.ScadaDeveloper}','{Departments.RnD}','{Ts}'),
        ('{Users.Fedorova}','Фёдорова Наталья Сергеевна','fedorova.ns@comdiag.ru','FEDOROVA.NS@COMDIAG.RU','$2a$13$JdqK6G8j5QAZREnlfIJ9PO1/kxmuDw2zTKzBwLmJDpYHFx7gT3rHq',1,true,'{Positions.TechnicalWriter}','{Departments.Documentation}','{Ts}'),
        ('{Users.Nikitin}','Никитин Алексей Владимирович','nikitin.av@comdiag.ru','NIKITIN.AV@COMDIAG.RU','$2a$13$uIvDvE5Y.1UHU5MNzMP2XuWJpE473spV4kqAKRWxEA0Dvjx2geqgq',1,true,'{Positions.ImplementationEngineer2}','{Departments.Implementation}','{Ts}')
        ON CONFLICT ("Id") DO NOTHING;
        """);

    private static void SeedProjects(MigrationBuilder mb) => mb.Sql($"""
        INSERT INTO "Projects" ("Id","Key","Name","Description","StartDate","DueDate","Color","Icon","OwnerId","IsArchived","Progress","DepartmentId","CreatedAt") VALUES
        ('{Projects.Crm24}','CRM24','Внедрение KomSync в ООО Комдиагностика','Пилот 20 мест, миграция знаний и задач из разрозненных инструментов','2026-02-01 00:00:00+00','2026-09-30 00:00:00+00','#2563eb','📊','{Users.Kozlov}',false,35,'{Departments.Implementation}','{Ts}'),
        ('{Projects.Int01}','INT01','Интеграция с биллингом','REST, очереди, мониторинг обмена','2026-01-15 00:00:00+00','2026-07-15 00:00:00+00','#7c3aed','🔗','{Users.Volkov}',false,20,'{Departments.Integration}','{Ts}'),
        ('{Projects.Sup02}','SUP02','Сопровождение релиза 4.2','Горячая линия, патчи, SLA заказчиков','2026-03-10 00:00:00+00','2026-06-10 00:00:00+00','#059669','🛠','{Users.Danilov}',false,55,'{Departments.Service}','{Ts}'),
        ('{Projects.Doc03}','DOC03','База знаний по ИВК САНПО','Статьи, видео, регламенты эксплуатации','2026-04-05 00:00:00+00','2026-12-31 00:00:00+00','#ea580c','📚','{Users.Fedorova}',false,40,'{Departments.Documentation}','{Ts}'),
        ('{Projects.GazpOrb}','GAZP-ORB','Вибромониторинг компрессоров','Газпром добыча Оренбург: КД619, пороги ISO 10816','2025-11-01 00:00:00+00','2026-08-30 00:00:00+00','#0ea5e9','⚙','{Users.Zhukov}',false,62,'{Departments.Implementation}','{Ts}'),
        ('{Projects.Kd722}','KD722','Серийный выпуск канала КД722','Абсолютная вибрация, удары, температура — серия 2026','2026-01-10 00:00:00+00','2026-10-01 00:00:00+00','#8b5cf6','📡','{Users.Medvedeva}',false,48,'{Departments.RnD}','{Ts}'),
        ('{Projects.Mems}','MEMS','Акселерометры КДМ3ХХ MEMS','Малогабаритные 1/2/3-осевые, замена импортных Bently/Metrix','2025-09-15 00:00:00+00','2026-11-30 00:00:00+00','#ec4899','🔬','{Users.Volkov}',false,71,'{Departments.RnD}','{Ts}'),
        ('{Projects.Sanpo}','SANPO','Внедрение ИВК САНПО','Рязанская НПК: сбор данных, архив, отчёты','2026-02-20 00:00:00+00','2026-09-15 00:00:00+00','#14b8a6','🖥','{Users.Sokolov}',false,33,'{Departments.Integration}','{Ts}'),
        ('{Projects.PazRel}','PAZ-REL','Вибровыключатель для ПАЗ','Управляемое реле тревоги, испытания на стенде','2026-03-01 00:00:00+00','2026-07-31 00:00:00+00','#ef4444','🚨','{Users.Medvedeva}',false,58,'{Departments.RnD}','{Ts}'),
        ('{Projects.NkuTnk}','NKU-TNK','НКУ для АСУ ТП ТАНЕКО','Шкаф автоматики, взрывозащита, монтаж на площадке','2026-04-01 00:00:00+00','2026-12-15 00:00:00+00','#f59e0b','🔌','{Users.Nikitin}',false,25,'{Departments.Integration}','{Ts}'),
        ('{Projects.UralTerm}','URAL-TERM','Термодиагностика УРАЛХИМ','Беспроводные датчики температуры, пилот на агрегате','2026-05-01 00:00:00+00','2026-11-01 00:00:00+00','#84cc16','🌡','{Users.Smirnov}',false,15,'{Departments.Implementation}','{Ts}'),
        ('{Projects.SiburDaq}','SIBUR-DAQ','Система сбора данных СИБУР','Окись этилена: опрос КД6407, интеграция в DCS','2026-03-15 00:00:00+00','2026-10-20 00:00:00+00','#6366f1','📈','{Users.Kravtsova}',false,42,'{Departments.Analytics}','{Ts}')
        ON CONFLICT ("Id") DO NOTHING;
        """);

    private static void SeedProjectMembers(MigrationBuilder mb) => mb.Sql($"""
        INSERT INTO "ProjectUser" ("MembersId","ProjectsId") VALUES
        ('{Users.DemoAdmin}','{Projects.Crm24}'),('{Users.Kozlov}','{Projects.Crm24}'),('{Users.Lebedeva}','{Projects.Crm24}'),('{Users.Petrov}','{Projects.Crm24}'),('{Users.Orlova}','{Projects.Crm24}'),
        ('{Users.DemoAdmin}','{Projects.GazpOrb}'),('{Users.Zhukov}','{Projects.GazpOrb}'),('{Users.Smirnov}','{Projects.GazpOrb}'),('{Users.Kravtsova}','{Projects.GazpOrb}'),('{Users.Lebedeva}','{Projects.GazpOrb}'),
        ('{Users.DemoAdmin}','{Projects.Kd722}'),('{Users.Medvedeva}','{Projects.Kd722}'),('{Users.Volkov}','{Projects.Kd722}'),('{Users.Nikitin}','{Projects.Kd722}'),
        ('{Users.DemoAdmin}','{Projects.Mems}'),('{Users.Medvedeva}','{Projects.Mems}'),('{Users.Volkov}','{Projects.Mems}'),('{Users.Sokolov}','{Projects.Mems}'),
        ('{Users.DemoAdmin}','{Projects.Sanpo}'),('{Users.Sokolov}','{Projects.Sanpo}'),('{Users.Petrov}','{Projects.Sanpo}'),('{Users.Fedorova}','{Projects.Sanpo}'),
        ('{Users.DemoAdmin}','{Projects.Int01}'),('{Users.Volkov}','{Projects.Int01}'),('{Users.Petrov}','{Projects.Int01}'),
        ('{Users.DemoAdmin}','{Projects.Sup02}'),('{Users.Danilov}','{Projects.Sup02}'),('{Users.Lebedeva}','{Projects.Sup02}'),
        ('{Users.DemoAdmin}','{Projects.Doc03}'),('{Users.Fedorova}','{Projects.Doc03}'),('{Users.Orlova}','{Projects.Doc03}'),
        ('{Users.DemoAdmin}','{Projects.PazRel}'),('{Users.Medvedeva}','{Projects.PazRel}'),('{Users.Smirnov}','{Projects.PazRel}'),
        ('{Users.DemoAdmin}','{Projects.NkuTnk}'),('{Users.Nikitin}','{Projects.NkuTnk}'),('{Users.Sokolov}','{Projects.NkuTnk}'),
        ('{Users.DemoAdmin}','{Projects.UralTerm}'),('{Users.Smirnov}','{Projects.UralTerm}'),('{Users.Zhukov}','{Projects.UralTerm}'),
        ('{Users.DemoAdmin}','{Projects.SiburDaq}'),('{Users.Kravtsova}','{Projects.SiburDaq}'),('{Users.Sokolov}','{Projects.SiburDaq}')
        ON CONFLICT DO NOTHING;
        """);

    private static void SeedStatusColumns(MigrationBuilder mb)
    {
        void Cols(Guid projectId, Guid todo, Guid progress, Guid review, Guid done) => mb.Sql($"""
            INSERT INTO "ProjectTaskStatusColumns" ("Id","ProjectId","Name","SortOrder","ColorHex","SemanticKind","IsDoneColumn","IsBlockedColumn") VALUES
            ('{todo}','{projectId}','Новые',0,'#94a3b8',0,false,false),
            ('{progress}','{projectId}','В работе',1,'#3b82f6',1,false,false),
            ('{review}','{projectId}','Проверка',2,'#f59e0b',2,false,false),
            ('{done}','{projectId}','Готово',3,'#22c55e',3,true,false)
            ON CONFLICT ("Id") DO NOTHING;
            """);

        Cols(Projects.Crm24, Columns.CrmTodo, Columns.CrmInProgress, Columns.CrmReview, Columns.CrmDone);
        Cols(Projects.Int01, Guid.Parse("c2000001-0001-4000-8000-000000000201"), Guid.Parse("c2000001-0002-4000-8000-000000000202"), Guid.Parse("c2000001-0003-4000-8000-000000000203"), Guid.Parse("c2000001-0004-4000-8000-000000000204"));
        Cols(Projects.Sup02, Guid.Parse("c2000002-0001-4000-8000-000000000201"), Guid.Parse("c2000002-0002-4000-8000-000000000202"), Guid.Parse("c2000002-0003-4000-8000-000000000203"), Guid.Parse("c2000002-0004-4000-8000-000000000204"));
        Cols(Projects.Doc03, Guid.Parse("c2000003-0001-4000-8000-000000000201"), Guid.Parse("c2000003-0002-4000-8000-000000000202"), Guid.Parse("c2000003-0003-4000-8000-000000000203"), Guid.Parse("c2000003-0004-4000-8000-000000000204"));
        Cols(Projects.GazpOrb, Guid.Parse("c2000004-0001-4000-8000-000000000201"), Guid.Parse("c2000004-0002-4000-8000-000000000202"), Guid.Parse("c2000004-0003-4000-8000-000000000203"), Guid.Parse("c2000004-0004-4000-8000-000000000204"));
        Cols(Projects.Kd722, Guid.Parse("c2000005-0001-4000-8000-000000000201"), Guid.Parse("c2000005-0002-4000-8000-000000000202"), Guid.Parse("c2000005-0003-4000-8000-000000000203"), Guid.Parse("c2000005-0004-4000-8000-000000000204"));
        Cols(Projects.Mems, Guid.Parse("c2000006-0001-4000-8000-000000000201"), Guid.Parse("c2000006-0002-4000-8000-000000000202"), Guid.Parse("c2000006-0003-4000-8000-000000000203"), Guid.Parse("c2000006-0004-4000-8000-000000000204"));
        Cols(Projects.Sanpo, Guid.Parse("c2000007-0001-4000-8000-000000000201"), Guid.Parse("c2000007-0002-4000-8000-000000000202"), Guid.Parse("c2000007-0003-4000-8000-000000000203"), Guid.Parse("c2000007-0004-4000-8000-000000000204"));
        Cols(Projects.PazRel, Guid.Parse("c2000008-0001-4000-8000-000000000201"), Guid.Parse("c2000008-0002-4000-8000-000000000202"), Guid.Parse("c2000008-0003-4000-8000-000000000203"), Guid.Parse("c2000008-0004-4000-8000-000000000204"));
        Cols(Projects.NkuTnk, Guid.Parse("c2000009-0001-4000-8000-000000000201"), Guid.Parse("c2000009-0002-4000-8000-000000000202"), Guid.Parse("c2000009-0003-4000-8000-000000000203"), Guid.Parse("c2000009-0004-4000-8000-000000000204"));
        Cols(Projects.UralTerm, Guid.Parse("c2000010-0001-4000-8000-000000000201"), Guid.Parse("c2000010-0002-4000-8000-000000000202"), Guid.Parse("c2000010-0003-4000-8000-000000000203"), Guid.Parse("c2000010-0004-4000-8000-000000000204"));
        Cols(Projects.SiburDaq, Guid.Parse("c2000011-0001-4000-8000-000000000201"), Guid.Parse("c2000011-0002-4000-8000-000000000202"), Guid.Parse("c2000011-0003-4000-8000-000000000203"), Guid.Parse("c2000011-0004-4000-8000-000000000204"));
    }

    private static void SeedTags(MigrationBuilder mb) => mb.Sql($"""
        INSERT INTO "Tags" ("Id","Name","ProjectId","CreatedAt") VALUES
        ('e1000001-0001-4000-8000-000000000001','пилот','{Projects.Crm24}','{Ts}'),
        ('e1000001-0002-4000-8000-000000000002','обучение','{Projects.Crm24}','{Ts}'),
        ('e1000002-0001-4000-8000-000000000001','интеграция','{Projects.Int01}','{Ts}'),
        ('e1000002-0002-4000-8000-000000000002','API','{Projects.Int01}','{Ts}'),
        ('e1000003-0001-4000-8000-000000000001','сопровождение','{Projects.Sup02}','{Ts}'),
        ('e1000004-0001-4000-8000-000000000001','документация','{Projects.Doc03}','{Ts}'),
        ('e1000005-0001-4000-8000-000000000001','Газпром','{Projects.GazpOrb}','{Ts}'),
        ('e1000005-0002-4000-8000-000000000002','вибрация','{Projects.GazpOrb}','{Ts}'),
        ('e1000006-0001-4000-8000-000000000001','серия','{Projects.Kd722}','{Ts}'),
        ('e1000007-0001-4000-8000-000000000001','MEMS','{Projects.Mems}','{Ts}'),
        ('e1000007-0002-4000-8000-000000000002','импортозамещение','{Projects.Mems}','{Ts}')
        ON CONFLICT ("Id") DO NOTHING;
        """);

    private static void SeedTasks(MigrationBuilder mb) => mb.Sql($"""
        INSERT INTO "Tasks" ("Id","Title","Description","ProjectTaskStatusColumnId","Priority","Deadline","CreatedAt","CreatorId","ProjectId","AssigneeId","ResponsibleId","TaskNumber","SortOrder") VALUES
        -- CRM24 (таблица Б.7)
        ('d1000001-0001-4000-8000-000000000001','Согласовать ТЗ с заказчиком','Версия 1.2, замечания до пятницы','{Columns.CrmInProgress}',2,'2026-04-18 00:00:00+00','{Ts}','{Users.Kozlov}','{Projects.Crm24}','{Users.Kozlov}','{Users.Lebedeva}',1,0),
        ('d1000001-0002-4000-8000-000000000002','Подготовить демо-стенд','Данные обезличены, 3 сценария','{Columns.CrmInProgress}',1,'2026-04-22 00:00:00+00','{Ts}','{Users.Kozlov}','{Projects.Crm24}','{Users.Petrov}','{Users.Lebedeva}',2,1),
        ('d1000001-0003-4000-8000-000000000003','Настроить роли в тесте','Матрица из приложения А','{Columns.CrmInProgress}',3,'2026-04-25 00:00:00+00','{Ts}','{Users.Kozlov}','{Projects.Crm24}','{Users.Orlova}','{Users.Volkov}',3,2),
        ('d1000001-0004-4000-8000-000000000004','Обновить регламент резервного копирования','Согласовать с ИБ','{Columns.CrmTodo}',0,'2026-05-02 00:00:00+00','{Ts}','{Users.Volkov}','{Projects.Crm24}','{Users.Volkov}','{Users.Volkov}',4,0),
        ('d1000001-0005-4000-8000-000000000005','Обучение ключевых пользователей','Группа из 12 человек, 2 потока','{Columns.CrmReview}',1,'2026-05-15 00:00:00+00','{Ts}','{Users.Lebedeva}','{Projects.Crm24}','{Users.Lebedeva}','{Users.Kozlov}',5,0),
        -- GAZP-ORB
        ('d2000004-0001-4000-8000-000000000001','Монтаж КД619 на К-101','Точки по схеме заказчика, кабельные трассы','c2000004-0002-4000-8000-000000000202',2,'2026-05-10 00:00:00+00','{Ts}','{Users.Zhukov}','{Projects.GazpOrb}','{Users.Smirnov}','{Users.Zhukov}',1,0),
        ('d2000004-0002-4000-8000-000000000002','Калибровка каналов вибрации','Эталонный стенд, протокол для ИБ','c2000004-0003-4000-8000-000000000203',2,'2026-05-20 00:00:00+00','{Ts}','{Users.Smirnov}','{Projects.GazpOrb}','{Users.Smirnov}','{Users.Kravtsova}',2,0),
        ('d2000004-0003-4000-8000-000000000003','Настройка порогов ISO 10816','Согласование с технологами Оренбурга','c2000004-0002-4000-8000-000000000202',3,'2026-04-28 00:00:00+00','{Ts}','{Users.Kravtsova}','{Projects.GazpOrb}','{Users.Kravtsova}','{Users.Zhukov}',3,1),
        ('d2000004-0004-4000-8000-000000000004','Акт ПНР компрессорной','Подпись заказчика, скан в проект','c2000004-0004-4000-8000-000000000204',1,'2026-06-01 00:00:00+00','{Ts}','{Users.Zhukov}','{Projects.GazpOrb}','{Users.Lebedeva}','{Users.Zhukov}',4,0),
        ('d2000004-0005-4000-8000-000000000005','VPN-доступ для удалённой диагностики','Заявка в ИБ Газпром добыча Оренбург','c2000004-0001-4000-8000-000000000201',1,'2026-04-30 00:00:00+00','{Ts}','{Users.Danilov}','{Projects.GazpOrb}','{Users.Danilov}','{Users.Smirnov}',5,0),
        -- KD722
        ('d2000005-0001-4000-8000-000000000001','Испытания термокамеры −40…+85','Цикл 72 ч, журнал измерений','c2000005-0002-4000-8000-000000000202',2,'2026-05-05 00:00:00+00','{Ts}','{Users.Medvedeva}','{Projects.Kd722}','{Users.Nikitin}','{Users.Medvedeva}',1,0),
        ('d2000005-0002-4000-8000-000000000002','Обновление прошивки КД722','Версия 2.4: фильтр ударных импульсов','c2000005-0003-4000-8000-000000000203',3,'2026-04-20 00:00:00+00','{Ts}','{Users.Volkov}','{Projects.Kd722}','{Users.Sokolov}','{Users.Volkov}',2,0),
        ('d2000005-0003-4000-8000-000000000003','Паспорт изделия — ревизия СТ-1','Согласование с ОТК','c2000005-0001-4000-8000-000000000201',1,'2026-05-12 00:00:00+00','{Ts}','{Users.Fedorova}','{Projects.Kd722}','{Users.Fedorova}','{Users.Medvedeva}',3,0),
        -- MEMS
        ('d2000006-0001-4000-8000-000000000001','Сравнение КДМ6ХХ с Bently Nevada','Отчёт для ТАИФ-НК','c2000006-0002-4000-8000-000000000202',2,'2026-05-18 00:00:00+00','{Ts}','{Users.Medvedeva}','{Projects.Mems}','{Users.Medvedeva}','{Users.Volkov}',1,0),
        ('d2000006-0002-4000-8000-000000000002','Взрывозащита Ex ia IIC T4','Заключение испытательной лаборатории','c2000006-0003-4000-8000-000000000203',3,'2026-06-15 00:00:00+00','{Ts}','{Users.Volkov}','{Projects.Mems}','{Users.Nikitin}','{Users.Medvedeva}',2,0),
        ('d2000006-0003-4000-8000-000000000003','3D-модель корпуса КДМ321','Для каталога и тендерной документации','c2000006-0001-4000-8000-000000000201',0,'2026-04-25 00:00:00+00','{Ts}','{Users.Sokolov}','{Projects.Mems}','{Users.Sokolov}','{Users.Volkov}',3,0),
        -- SANPO
        ('d2000007-0001-4000-8000-000000000001','Развёртывание сервера архивации','РНПК, VLAN диагностики','c2000007-0002-4000-8000-000000000202',2,'2026-05-08 00:00:00+00','{Ts}','{Users.Sokolov}','{Projects.Sanpo}','{Users.Petrov}','{Users.Sokolov}',1,0),
        ('d2000007-0002-4000-8000-000000000002','Импорт трендов за 2024–2025','Миграция из legacy-системы','c2000007-0001-4000-8000-000000000201',1,'2026-06-01 00:00:00+00','{Ts}','{Users.Petrov}','{Projects.Sanpo}','{Users.Petrov}','{Users.Kravtsova}',2,0),
        -- INT01 / SUP02
        ('d2000001-0001-4000-8000-000000000001','Контракт REST API v2','OpenAPI, авторизация JWT','c2000001-0002-4000-8000-000000000202',2,'2026-04-15 00:00:00+00','{Ts}','{Users.Volkov}','{Projects.Int01}','{Users.Petrov}','{Users.Volkov}',1,0),
        ('d2000002-0001-4000-8000-000000000001','Патч 4.2.1 — утечка памяти hub','Hotfix для заказчиков Сибура','c2000002-0002-4000-8000-000000000202',3,'2026-04-12 00:00:00+00','{Ts}','{Users.Danilov}','{Projects.Sup02}','{Users.Sokolov}','{Users.Danilov}',1,0),
        ('d2000002-0002-4000-8000-000000000002','Регламент эскалации L2','Согласование с сервисным отделом','c2000002-0001-4000-8000-000000000201',1,'2026-05-01 00:00:00+00','{Ts}','{Users.Danilov}','{Projects.Sup02}','{Users.Lebedeva}','{Users.Danilov}',2,0),
        -- PAZ / NKU / URAL / SIBUR
        ('d2000008-0001-4000-8000-000000000001','Стендовые испытания вибровыключателя','1000 циклов срабатывания','c2000008-0002-4000-8000-000000000202',2,'2026-05-22 00:00:00+00','{Ts}','{Users.Medvedeva}','{Projects.PazRel}','{Users.Smirnov}','{Users.Medvedeva}',1,0),
        ('d2000009-0001-4000-8000-000000000001','Схема НКУ ТАНЕКО','Единолинейная + спецификация','c2000009-0001-4000-8000-000000000201',1,'2026-05-30 00:00:00+00','{Ts}','{Users.Nikitin}','{Projects.NkuTnk}','{Users.Nikitin}','{Users.Sokolov}',1,0),
        ('d2000010-0001-4000-8000-000000000001','Пилот беспроводных датчиков','УРАЛХИМ: аммиачный агрегат','c2000010-0002-4000-8000-000000000202',2,'2026-06-10 00:00:00+00','{Ts}','{Users.Smirnov}','{Projects.UralTerm}','{Users.Smirnov}','{Users.Zhukov}',1,0),
        ('d2000011-0001-4000-8000-000000000001','Опрос КД6407 по Modbus TCP','СИБУР-Нефтехим, цех гликолей','c2000011-0002-4000-8000-000000000202',2,'2026-05-14 00:00:00+00','{Ts}','{Users.Kravtsova}','{Projects.SiburDaq}','{Users.Sokolov}','{Users.Kravtsova}',1,0)
        ON CONFLICT ("Id") DO NOTHING;
        """);

    private static void SeedProjectComments(MigrationBuilder mb) => mb.Sql($"""
        INSERT INTO "ProjectComments" ("Id","ProjectId","Content","AuthorId","CreatedAt") VALUES
        ('c3000001-0001-4000-8000-000000000001','{Projects.Crm24}','Демо переносим на вторник 15:00, обновите календарь встречи с заказчиком.','{Users.Kozlov}','{Ts}'),
        ('c3000001-0002-4000-8000-000000000002','{Projects.Crm24}','Протокол от 08.04 выложен в общую папку проекта.','{Users.Lebedeva}','{Ts}'),
        ('c3000001-0003-4000-8000-000000000003','{Projects.Crm24}','Нужен доступ VPN для подрядчика «Сфера» до конца недели.','{Users.Petrov}','{Ts}'),
        ('c3000001-0004-4000-8000-000000000004','{Projects.Crm24}','Бюджет на обучение согласован в рамках 120 тыс. без НДС.','{Users.Zhukov}','{Ts}'),
        ('c3000001-0005-4000-8000-000000000005','{Projects.Crm24}','Риск: задержка поставки лицензий, предлагаю временные ключи.','{Users.Volkov}','{Ts}'),
        ('c3000004-0001-4000-8000-000000000001','{Projects.GazpOrb}','Заказчик подтвердил график останова К-101 на 12–14 мая.','{Users.Zhukov}','{Ts}'),
        ('c3000004-0002-4000-8000-000000000002','{Projects.GazpOrb}','Кабель КИП-группы проложен, ждём маркировку от смежников.','{Users.Smirnov}','{Ts}'),
        ('c3000005-0001-4000-8000-000000000001','{Projects.Kd722}','ОТК запросил доп. испытания на виброустойчивость — закладываем 3 дня.','{Users.Medvedeva}','{Ts}'),
        ('c3000006-0001-4000-8000-000000000001','{Projects.Mems}','Образцы КДМ6ХХ отправлены в ТАИФ-НК на сравнительные испытания.','{Users.Volkov}','{Ts}'),
        ('c3000007-0001-4000-8000-000000000001','{Projects.Sanpo}','РНПК выделила VLAN — можно начинать развёртывание архива.','{Users.Sokolov}','{Ts}')
        ON CONFLICT ("Id") DO NOTHING;
        """);

    private static void SeedTaskComments(MigrationBuilder mb) => mb.Sql($"""
        INSERT INTO "TaskComments" ("Id","TaskId","UserId","Content","CreatedAt","UpdatedAt") VALUES
        ('c4000001-0001-4000-8000-000000000001','d1000001-0001-4000-8000-000000000001','{Users.Lebedeva}','Черновик отчёта в weekly_v3.docx, жду ревью до 17:00.','{Ts}','{Ts}'),
        ('c4000001-0002-4000-8000-000000000002','d1000001-0002-4000-8000-000000000002','{Users.Petrov}','Блокер снят: тестовый контур подняли, можно продолжать.','{Ts}','{Ts}'),
        ('c4000001-0003-4000-8000-000000000003','d1000001-0003-4000-8000-000000000003','{Users.Orlova}','Добавил скриншот ошибки в вложении, воспроизводится на Chrome 134.','{Ts}','{Ts}'),
        ('c4000001-0004-4000-8000-000000000004','d1000001-0004-4000-8000-000000000004','{Users.Volkov}','Согласен с оценкой 8 ч, приступаю после merge ветки feature/api-sync.','{Ts}','{Ts}'),
        ('c4000001-0005-4000-8000-000000000001','d1000001-0001-4000-8000-000000000001','{Users.Kozlov}','Заказчик подтвердил формулировку п. 4.2, можно закрывать задачу.','{Ts}','{Ts}'),
        ('c4000004-0001-4000-8000-000000000001','d2000004-0001-4000-8000-000000000001','{Users.Smirnov}','Крепёж M8 на фундамент согласован с механиками площадки.','{Ts}','{Ts}'),
        ('c4000004-0002-4000-8000-000000000002','d2000004-0003-4000-8000-000000000003','{Users.Kravtsova}','Пороги предупреждения/тревоги вынесены на согласование в среду.','{Ts}','{Ts}'),
        ('c4000005-0001-4000-8000-000000000001','d2000005-0002-4000-8000-000000000002','{Users.Sokolov}','Прошивка 2.4 прошла smoke на стенде НИОКР.','{Ts}','{Ts}'),
        ('c4000006-0001-4000-8000-000000000001','d2000006-0001-4000-8000-000000000001','{Users.Medvedeva}','Таблица сравнения чувствительности готова на 80%.','{Ts}','{Ts}'),
        ('c4000002-0001-4000-8000-000000000001','d2000002-0001-4000-8000-000000000001','{Users.Sokolov}','Патч выкатили на тестовый контур СИБУР, мониторим 48 ч.','{Ts}','{Ts}')
        ON CONFLICT ("Id") DO NOTHING;
        """);

    private static void SeedKnowledgeArticles(MigrationBuilder mb) => mb.Sql($"""
        INSERT INTO "KnowledgeArticles" ("Id","Title","Slug","ContentMarkdown","ParentId","ProjectId","AuthorId","SortOrder","CreatedAt") VALUES
        ('f1000001-0001-4000-8000-000000000001','Чек-лист приёмки спринта','chek-list-priemki',E'## Цель\n- [ ] Задачи в «Закрыто»',NULL,'{Projects.Crm24}','{Users.Kozlov}',10,'{Ts}'),
        ('f1000001-0002-4000-8000-000000000002','Регламент деплоя на тест','deploy-test-regl',E'1. Тег\n2. Pipeline\n3. Smoke',NULL,'{Projects.Int01}','{Users.Volkov}',20,'{Ts}'),
        ('f1000001-0003-4000-8000-000000000003','Частые ошибки интеграции','integration-faq',E'### Таймаут\nУвеличить readTimeout',NULL,'{Projects.Sup02}','{Users.Petrov}',30,'{Ts}'),
        ('f1000001-0004-4000-8000-000000000004','Описание поля «Контрагент»','pole-kontragent','Типы: ЮЛ, ИП, физлицо',NULL,NULL,'{Users.Fedorova}',5,'{Ts}'),
        ('f1000001-0005-4000-8000-000000000005','Шаблон письма заказчику','shablon-pismo','Уважаемые коллеги, направляем…',NULL,'{Projects.Doc03}','{Users.Fedorova}',40,'{Ts}'),
        ('f1000002-0001-4000-8000-000000000001','Монтаж акселерометра КД619','montazh-kd619',E'## Требования\n- Момент 8 Н·м\n- Заземление экрана',NULL,'{Projects.GazpOrb}','{Users.Smirnov}',10,'{Ts}'),
        ('f1000002-0002-4000-8000-000000000002','Пороги ISO 10816 для компрессоров','iso-10816-komp',E'Класс II: 4.5 мм/с предупреждение\nКласс III: 7.1 мм/с авария',NULL,'{Projects.GazpOrb}','{Users.Kravtsova}',20,'{Ts}'),
        ('f1000003-0001-4000-8000-000000000001','КД722 — руководство по эксплуатации','kd722-rukovodstvo','Канал измерения: вибрация, удары, температура',NULL,'{Projects.Kd722}','{Users.Fedorova}',10,'{Ts}'),
        ('f1000004-0001-4000-8000-000000000001','MEMS vs ICP: выбор датчика','mems-vs-icp',E'| Критерий | MEMS | ICP |\n| Чувствительность | средняя | высокая |',NULL,'{Projects.Mems}','{Users.Medvedeva}',10,'{Ts}'),
        ('f1000005-0001-4000-8000-000000000001','ИВК САНПО — архитектура','ivk-sanpo-arch',E'Сервер сбора → архив → отчёты\nКлиент оператора — тонкий.',NULL,'{Projects.Sanpo}','{Users.Sokolov}',10,'{Ts}'),
        ('f1000006-0001-4000-8000-000000000001','Чек-лист выездного обследования','vyezd-obled-checklist',E'- СИЗ\n- Калибровка переносного ВИБ-8\n- Фото точек монтажа',NULL,NULL,'{Users.Smirnov}',15,'{Ts}'),
        ('f1000006-0002-4000-8000-000000000002','Взрывозащищённые шкафы — типовые решения','shkafy-ex-ia',E'Ex ia IIC T4\nКабельные вводы M20×1.5',NULL,'{Projects.NkuTnk}','{Users.Nikitin}',25,'{Ts}'),
        ('f1000007-0001-4000-8000-000000000001','Адаптация нового инженера','onboarding-inzhener',E'1. Охрана труда\n2. KomSync\n3. Стажировка на объекте',NULL,'{Projects.Crm24}','{Users.Lebedeva}',50,'{Ts}'),
        ('f1000008-0001-4000-8000-000000000001','Modbus TCP — карта регистров КД6407','modbus-kd6407','Holding 40001 — RMS вибрации',NULL,'{Projects.SiburDaq}','{Users.Kravtsova}',10,'{Ts}')
        ON CONFLICT ("Id") DO NOTHING;
        """);
}
