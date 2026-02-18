using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIndustry.Repository.Migrations
{
    /// <inheritdoc />
    public partial class SeedDeepCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Derin kategori hiyerarşisi oluştur - bağımsız olarak çalışır
            // Parent yoksa oluşturur, varsa mevcut ID'yi kullanır
            
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    -- Ana kategoriler (1. seviye)
                    yedek_parca_id UUID;
                    hidrolik_id UUID;
                    elektrik_id UUID;
                    
                    -- 2. seviye kategoriler
                    motor_yedek_id UUID;
                    hidrolik_pompa_id UUID;
                    plc_id UUID;
                    
                    -- 3. seviye kategoriler
                    cekici_id UUID;
                    kamyon_id UUID;
                    otobus_id UUID;
                    disli_pompa_id UUID;
                    pistonlu_pompa_id UUID;
                    paletli_pompa_id UUID;
                    siemens_id UUID;
                    omron_id UUID;
                    allen_id UUID;
                    mitsubishi_id UUID;
                    
                    -- 4. seviye kategoriler
                    scania_id UUID;
                    volvo_id UUID;
                    mercedes_id UUID;
                    man_id UUID;
                    daf_id UUID;
                BEGIN
                    -- =====================================================
                    -- 1. SEVİYE: Ana Kategoriler (yoksa oluştur)
                    -- =====================================================
                    
                    SELECT ""Id"" INTO yedek_parca_id FROM ""Categories"" WHERE ""Name"" = 'Yedek Parça' AND ""ParentId"" IS NULL LIMIT 1;
                    IF yedek_parca_id IS NULL THEN
                        yedek_parca_id := gen_random_uuid();
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (yedek_parca_id, 'Yedek Parça', 'Endüstriyel yedek parçalar ve komponentler', true, NULL, NOW());
                        RAISE NOTICE 'Created: Yedek Parça (L1)';
                    END IF;
                    
                    SELECT ""Id"" INTO hidrolik_id FROM ""Categories"" WHERE ""Name"" = 'Hidrolik Sistemler' AND ""ParentId"" IS NULL LIMIT 1;
                    IF hidrolik_id IS NULL THEN
                        hidrolik_id := gen_random_uuid();
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (hidrolik_id, 'Hidrolik Sistemler', 'Hidrolik pompalar, silindirler ve sistemler', true, NULL, NOW());
                        RAISE NOTICE 'Created: Hidrolik Sistemler (L1)';
                    END IF;
                    
                    SELECT ""Id"" INTO elektrik_id FROM ""Categories"" WHERE ""Name"" = 'Elektrik ve Otomasyon' AND ""ParentId"" IS NULL LIMIT 1;
                    IF elektrik_id IS NULL THEN
                        elektrik_id := gen_random_uuid();
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (elektrik_id, 'Elektrik ve Otomasyon', 'Elektrik malzemeleri ve otomasyon sistemleri', true, NULL, NOW());
                        RAISE NOTICE 'Created: Elektrik ve Otomasyon (L1)';
                    END IF;
                    
                    -- =====================================================
                    -- 2. SEVİYE: Alt Kategoriler
                    -- =====================================================
                    
                    SELECT ""Id"" INTO motor_yedek_id FROM ""Categories"" WHERE ""Name"" = 'Motor Yedek Parçaları' AND ""ParentId"" = yedek_parca_id LIMIT 1;
                    IF motor_yedek_id IS NULL THEN
                        motor_yedek_id := gen_random_uuid();
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (motor_yedek_id, 'Motor Yedek Parçaları', 'Elektrik ve dizel motor parçaları', true, yedek_parca_id, NOW());
                        RAISE NOTICE 'Created: Motor Yedek Parçaları (L2)';
                    END IF;
                    
                    SELECT ""Id"" INTO hidrolik_pompa_id FROM ""Categories"" WHERE ""Name"" = 'Hidrolik Pompalar' AND ""ParentId"" = hidrolik_id LIMIT 1;
                    IF hidrolik_pompa_id IS NULL THEN
                        hidrolik_pompa_id := gen_random_uuid();
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (hidrolik_pompa_id, 'Hidrolik Pompalar', 'Dişli, pistonlu ve paletli pompalar', true, hidrolik_id, NOW());
                        RAISE NOTICE 'Created: Hidrolik Pompalar (L2)';
                    END IF;
                    
                    SELECT ""Id"" INTO plc_id FROM ""Categories"" WHERE ""Name"" = 'PLC ve Kontrol' AND ""ParentId"" = elektrik_id LIMIT 1;
                    IF plc_id IS NULL THEN
                        plc_id := gen_random_uuid();
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (plc_id, 'PLC ve Kontrol', 'PLC sistemleri ve kontrol panelleri', true, elektrik_id, NOW());
                        RAISE NOTICE 'Created: PLC ve Kontrol (L2)';
                    END IF;
                    
                    -- =====================================================
                    -- 3. SEVİYE: Araç Tipleri / Pompa Tipleri / Markalar
                    -- =====================================================
                    
                    -- Motor Yedek Parçaları altına araç tipleri
                    IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Çekici' AND ""ParentId"" = motor_yedek_id) THEN
                        cekici_id := gen_random_uuid();
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (cekici_id, 'Çekici', 'Çekici motor yedek parçaları', true, motor_yedek_id, NOW());
                        RAISE NOTICE 'Created: Çekici (L3)';
                    ELSE
                        SELECT ""Id"" INTO cekici_id FROM ""Categories"" WHERE ""Name"" = 'Çekici' AND ""ParentId"" = motor_yedek_id;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Kamyon' AND ""ParentId"" = motor_yedek_id) THEN
                        kamyon_id := gen_random_uuid();
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (kamyon_id, 'Kamyon', 'Kamyon motor yedek parçaları', true, motor_yedek_id, NOW());
                        RAISE NOTICE 'Created: Kamyon (L3)';
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Otobüs' AND ""ParentId"" = motor_yedek_id) THEN
                        otobus_id := gen_random_uuid();
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (otobus_id, 'Otobüs', 'Otobüs motor yedek parçaları', true, motor_yedek_id, NOW());
                        RAISE NOTICE 'Created: Otobüs (L3)';
                    END IF;
                    
                    -- Hidrolik Pompalar altına pompa tipleri
                    IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Dişli Pompalar' AND ""ParentId"" = hidrolik_pompa_id) THEN
                        disli_pompa_id := gen_random_uuid();
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (disli_pompa_id, 'Dişli Pompalar', 'Dişli tip hidrolik pompalar', true, hidrolik_pompa_id, NOW());
                        RAISE NOTICE 'Created: Dişli Pompalar (L3)';
                    ELSE
                        SELECT ""Id"" INTO disli_pompa_id FROM ""Categories"" WHERE ""Name"" = 'Dişli Pompalar' AND ""ParentId"" = hidrolik_pompa_id;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Pistonlu Pompalar' AND ""ParentId"" = hidrolik_pompa_id) THEN
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (gen_random_uuid(), 'Pistonlu Pompalar', 'Pistonlu tip hidrolik pompalar', true, hidrolik_pompa_id, NOW());
                        RAISE NOTICE 'Created: Pistonlu Pompalar (L3)';
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Paletli Pompalar' AND ""ParentId"" = hidrolik_pompa_id) THEN
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (gen_random_uuid(), 'Paletli Pompalar', 'Paletli tip hidrolik pompalar', true, hidrolik_pompa_id, NOW());
                        RAISE NOTICE 'Created: Paletli Pompalar (L3)';
                    END IF;
                    
                    -- PLC altına markalar
                    IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Siemens' AND ""ParentId"" = plc_id) THEN
                        siemens_id := gen_random_uuid();
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (siemens_id, 'Siemens', 'Siemens PLC sistemleri', true, plc_id, NOW());
                        RAISE NOTICE 'Created: Siemens (L3)';
                    ELSE
                        SELECT ""Id"" INTO siemens_id FROM ""Categories"" WHERE ""Name"" = 'Siemens' AND ""ParentId"" = plc_id;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Omron' AND ""ParentId"" = plc_id) THEN
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (gen_random_uuid(), 'Omron', 'Omron PLC sistemleri', true, plc_id, NOW());
                        RAISE NOTICE 'Created: Omron (L3)';
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Allen Bradley' AND ""ParentId"" = plc_id) THEN
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (gen_random_uuid(), 'Allen Bradley', 'Allen Bradley PLC sistemleri', true, plc_id, NOW());
                        RAISE NOTICE 'Created: Allen Bradley (L3)';
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Mitsubishi' AND ""ParentId"" = plc_id) THEN
                        INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                        VALUES (gen_random_uuid(), 'Mitsubishi', 'Mitsubishi PLC sistemleri', true, plc_id, NOW());
                        RAISE NOTICE 'Created: Mitsubishi (L3)';
                    END IF;
                    
                    -- =====================================================
                    -- 4. SEVİYE: Çekici Markaları / Pompa Markaları / Siemens Serileri
                    -- =====================================================
                    
                    -- Çekici altına markalar
                    IF cekici_id IS NOT NULL THEN
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Scania' AND ""ParentId"" = cekici_id) THEN
                            scania_id := gen_random_uuid();
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (scania_id, 'Scania', 'Scania çekici yedek parçaları', true, cekici_id, NOW());
                            RAISE NOTICE 'Created: Scania (L4)';
                        ELSE
                            SELECT ""Id"" INTO scania_id FROM ""Categories"" WHERE ""Name"" = 'Scania' AND ""ParentId"" = cekici_id;
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Volvo' AND ""ParentId"" = cekici_id) THEN
                            volvo_id := gen_random_uuid();
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (volvo_id, 'Volvo', 'Volvo çekici yedek parçaları', true, cekici_id, NOW());
                            RAISE NOTICE 'Created: Volvo (L4)';
                        ELSE
                            SELECT ""Id"" INTO volvo_id FROM ""Categories"" WHERE ""Name"" = 'Volvo' AND ""ParentId"" = cekici_id;
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Mercedes' AND ""ParentId"" = cekici_id) THEN
                            mercedes_id := gen_random_uuid();
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (mercedes_id, 'Mercedes', 'Mercedes çekici yedek parçaları', true, cekici_id, NOW());
                            RAISE NOTICE 'Created: Mercedes (L4)';
                        ELSE
                            SELECT ""Id"" INTO mercedes_id FROM ""Categories"" WHERE ""Name"" = 'Mercedes' AND ""ParentId"" = cekici_id;
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'MAN' AND ""ParentId"" = cekici_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'MAN', 'MAN çekici yedek parçaları', true, cekici_id, NOW());
                            RAISE NOTICE 'Created: MAN (L4)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'DAF' AND ""ParentId"" = cekici_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'DAF', 'DAF çekici yedek parçaları', true, cekici_id, NOW());
                            RAISE NOTICE 'Created: DAF (L4)';
                        END IF;
                    END IF;
                    
                    -- Dişli Pompalar altına markalar
                    IF disli_pompa_id IS NOT NULL THEN
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Bosch Rexroth' AND ""ParentId"" = disli_pompa_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'Bosch Rexroth', 'Bosch Rexroth dişli pompalar', true, disli_pompa_id, NOW());
                            RAISE NOTICE 'Created: Bosch Rexroth (L4)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Parker' AND ""ParentId"" = disli_pompa_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'Parker', 'Parker dişli pompalar', true, disli_pompa_id, NOW());
                            RAISE NOTICE 'Created: Parker (L4)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Casappa' AND ""ParentId"" = disli_pompa_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'Casappa', 'Casappa dişli pompalar', true, disli_pompa_id, NOW());
                            RAISE NOTICE 'Created: Casappa (L4)';
                        END IF;
                    END IF;
                    
                    -- Siemens altına seriler
                    IF siemens_id IS NOT NULL THEN
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'S7-1200' AND ""ParentId"" = siemens_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'S7-1200', 'Siemens S7-1200 serisi', true, siemens_id, NOW());
                            RAISE NOTICE 'Created: S7-1200 (L4)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'S7-1500' AND ""ParentId"" = siemens_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'S7-1500', 'Siemens S7-1500 serisi', true, siemens_id, NOW());
                            RAISE NOTICE 'Created: S7-1500 (L4)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'S7-300' AND ""ParentId"" = siemens_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'S7-300', 'Siemens S7-300 serisi', true, siemens_id, NOW());
                            RAISE NOTICE 'Created: S7-300 (L4)';
                        END IF;
                    END IF;
                    
                    -- =====================================================
                    -- 5. SEVİYE: Scania/Volvo/Mercedes Modelleri
                    -- =====================================================
                    
                    -- Scania modelleri
                    IF scania_id IS NOT NULL THEN
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'R410' AND ""ParentId"" = scania_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'R410', 'Scania R410 yedek parçaları', true, scania_id, NOW());
                            RAISE NOTICE 'Created: R410 (L5)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'R450' AND ""ParentId"" = scania_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'R450', 'Scania R450 yedek parçaları', true, scania_id, NOW());
                            RAISE NOTICE 'Created: R450 (L5)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'R500' AND ""ParentId"" = scania_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'R500', 'Scania R500 yedek parçaları', true, scania_id, NOW());
                            RAISE NOTICE 'Created: R500 (L5)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'S500' AND ""ParentId"" = scania_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'S500', 'Scania S500 yedek parçaları', true, scania_id, NOW());
                            RAISE NOTICE 'Created: S500 (L5)';
                        END IF;
                    END IF;
                    
                    -- Volvo modelleri
                    IF volvo_id IS NOT NULL THEN
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'FH16' AND ""ParentId"" = volvo_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'FH16', 'Volvo FH16 yedek parçaları', true, volvo_id, NOW());
                            RAISE NOTICE 'Created: FH16 (L5)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'FH500' AND ""ParentId"" = volvo_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'FH500', 'Volvo FH500 yedek parçaları', true, volvo_id, NOW());
                            RAISE NOTICE 'Created: FH500 (L5)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'FM' AND ""ParentId"" = volvo_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'FM', 'Volvo FM yedek parçaları', true, volvo_id, NOW());
                            RAISE NOTICE 'Created: FM (L5)';
                        END IF;
                    END IF;
                    
                    -- Mercedes modelleri
                    IF mercedes_id IS NOT NULL THEN
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Actros' AND ""ParentId"" = mercedes_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'Actros', 'Mercedes Actros yedek parçaları', true, mercedes_id, NOW());
                            RAISE NOTICE 'Created: Actros (L5)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Arocs' AND ""ParentId"" = mercedes_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'Arocs', 'Mercedes Arocs yedek parçaları', true, mercedes_id, NOW());
                            RAISE NOTICE 'Created: Arocs (L5)';
                        END IF;
                        
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Axor' AND ""ParentId"" = mercedes_id) THEN
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"")
                            VALUES (gen_random_uuid(), 'Axor', 'Mercedes Axor yedek parçaları', true, mercedes_id, NOW());
                            RAISE NOTICE 'Created: Axor (L5)';
                        END IF;
                    END IF;
                    
                    RAISE NOTICE 'Deep category seeding completed!';
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- 5. seviyeden başlayarak geriye doğru sil
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('R410', 'R450', 'R500', 'S500', 'FH16', 'FH500', 'FM', 'Actros', 'Arocs', 'Axor');
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('Scania', 'Volvo', 'Mercedes', 'MAN', 'DAF', 'Bosch Rexroth', 'Parker', 'Casappa', 'S7-1200', 'S7-1500', 'S7-300');
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('Çekici', 'Kamyon', 'Otobüs', 'Dişli Pompalar', 'Pistonlu Pompalar', 'Paletli Pompalar', 'Siemens', 'Omron', 'Allen Bradley', 'Mitsubishi');
            ");
        }
    }
}
