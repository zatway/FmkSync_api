namespace Infrastructure.Persistence.Seed;

/// <summary>Фиксированные идентификаторы демо-данных (миграция SeedDemoData).</summary>
internal static class DemoSeedIds
{
    internal static readonly DateTimeOffset SeedCreatedAt = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    internal static class Departments
    {
        public static readonly Guid Implementation = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        public static readonly Guid Integration = Guid.Parse("a2c3d4e5-f6a7-8901-bcde-f1234567890a");
        public static readonly Guid Service = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012");
        public static readonly Guid Documentation = Guid.Parse("d4e5f6a7-b8c9-0123-def0-234567890123");
        public static readonly Guid Analytics = Guid.Parse("f6a7b8c9-d0e1-2345-f012-456789012345");
        public static readonly Guid RnD = Guid.Parse("b8c9d0e1-f2a3-4567-1234-678901234567");
        public static readonly Guid DeploymentSupport = Guid.Parse("d7e8f9a0-b1c2-3456-7890-abcdef012345");
        public static readonly Guid CrmCompetence = Guid.Parse("e8f9a0b1-c2d3-4567-8901-bcdef0123456");
        public static readonly Guid Commercial = Guid.Parse("f9a0b1c2-d3e4-5678-9012-cdef01234567");
    }

    internal static class Positions
    {
        public static readonly Guid ProjectLead = Guid.Parse("01000000-0000-4000-8000-000000000001");
        public static readonly Guid ImplementationSpecialist = Guid.Parse("01000000-0000-4000-8000-000000000002");
        public static readonly Guid IntegrationEngineer = Guid.Parse("01000000-0000-4000-8000-000000000003");
        public static readonly Guid Analyst = Guid.Parse("01000000-0000-4000-8000-000000000004");
        public static readonly Guid DevLead = Guid.Parse("01000000-0000-4000-8000-000000000005");
        public static readonly Guid ImplementationEngineer2 = Guid.Parse("01000000-0000-4000-8000-000000000006");
        public static readonly Guid Analyst1C = Guid.Parse("01000000-0000-4000-8000-000000000007");
        public static readonly Guid VibroEngineer = Guid.Parse("01000000-0000-4000-8000-000000000008");
        public static readonly Guid SensorRnD = Guid.Parse("01000000-0000-4000-8000-000000000009");
        public static readonly Guid ServiceEngineer = Guid.Parse("01000000-0000-4000-8000-00000000000a");
        public static readonly Guid SalesManager = Guid.Parse("01000000-0000-4000-8000-00000000000b");
        public static readonly Guid ScadaDeveloper = Guid.Parse("01000000-0000-4000-8000-00000000000c");
        public static readonly Guid TechnicalWriter = Guid.Parse("01000000-0000-4000-8000-00000000000d");
    }

    internal static class Users
    {
        public static readonly Guid DemoAdmin = Guid.Parse("f0e0d0c0-b0a0-4000-a000-000000000099");
        public static readonly Guid Kozlov = Guid.Parse("a1b1c1d1-e1f1-4111-a111-111111111111");
        public static readonly Guid Lebedeva = Guid.Parse("b2b2c2d2-e2f2-4222-a222-222222222222");
        public static readonly Guid Petrov = Guid.Parse("c3c3c3c3-e3f3-4333-a333-333333333333");
        public static readonly Guid Orlova = Guid.Parse("d4d4d4d4-e4f4-4444-a444-444444444444");
        public static readonly Guid Volkov = Guid.Parse("e5e5e5e5-e5f5-4555-a555-555555555555");
        public static readonly Guid Smirnov = Guid.Parse("11111111-1111-4111-a111-111111111101");
        public static readonly Guid Kravtsova = Guid.Parse("11111111-1111-4111-a111-111111111102");
        public static readonly Guid Danilov = Guid.Parse("11111111-1111-4111-a111-111111111103");
        public static readonly Guid Zhukov = Guid.Parse("11111111-1111-4111-a111-111111111104");
        public static readonly Guid Medvedeva = Guid.Parse("11111111-1111-4111-a111-111111111105");
        public static readonly Guid Sokolov = Guid.Parse("11111111-1111-4111-a111-111111111106");
        public static readonly Guid Fedorova = Guid.Parse("11111111-1111-4111-a111-111111111107");
        public static readonly Guid Nikitin = Guid.Parse("11111111-1111-4111-a111-111111111108");

        public static readonly Guid[] All =
        [
            DemoAdmin, Kozlov, Lebedeva, Petrov, Orlova, Volkov,
            Smirnov, Kravtsova, Danilov, Zhukov, Medvedeva, Sokolov, Fedorova, Nikitin
        ];
    }

    internal static class Projects
    {
        public static readonly Guid Crm24 = Guid.Parse("b1000001-0001-4000-8000-000000000001");
        public static readonly Guid Int01 = Guid.Parse("b1000001-0002-4000-8000-000000000002");
        public static readonly Guid Sup02 = Guid.Parse("b1000001-0003-4000-8000-000000000003");
        public static readonly Guid Doc03 = Guid.Parse("b1000001-0004-4000-8000-000000000004");
        public static readonly Guid GazpOrb = Guid.Parse("b1000001-0005-4000-8000-000000000005");
        public static readonly Guid Kd722 = Guid.Parse("b1000001-0006-4000-8000-000000000006");
        public static readonly Guid Mems = Guid.Parse("b1000001-0007-4000-8000-000000000007");
        public static readonly Guid Sanpo = Guid.Parse("b1000001-0008-4000-8000-000000000008");
        public static readonly Guid PazRel = Guid.Parse("b1000001-0009-4000-8000-000000000009");
        public static readonly Guid NkuTnk = Guid.Parse("b1000001-000a-4000-8000-00000000000a");
        public static readonly Guid UralTerm = Guid.Parse("b1000001-000b-4000-8000-00000000000b");
        public static readonly Guid SiburDaq = Guid.Parse("b1000001-000c-4000-8000-00000000000c");

        public static readonly Guid[] All =
        [
            Crm24, Int01, Sup02, Doc03, GazpOrb, Kd722, Mems, Sanpo, PazRel, NkuTnk, UralTerm, SiburDaq
        ];
    }

    internal static class Columns
    {
        public static readonly Guid CrmTodo = Guid.Parse("f1e2d3c4-b5a6-7890-fedc-ba9876543201");
        public static readonly Guid CrmInProgress = Guid.Parse("f1e2d3c4-b5a6-7890-fedc-ba9876543210");
        public static readonly Guid CrmReview = Guid.Parse("f1e2d3c4-b5a6-7890-fedc-ba9876543203");
        public static readonly Guid CrmDone = Guid.Parse("f1e2d3c4-b5a6-7890-fedc-ba9876543204");
    }
}
