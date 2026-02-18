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
            // Motor Yedek Parçaları altına derin kategoriler ekle
            // Önce mevcut "Motor Yedek Parçaları" kategorisinin ID'sini bul ve alt kategorileri ekle
            
            migrationBuilder.Sql(@"
                -- Motor Yedek Parçaları kategorisinin ID'sini al
                DO $$
                DECLARE
                    motor_yedek_id UUID;
                    cekici_id UUID := gen_random_uuid();
                    kamyon_id UUID := gen_random_uuid();
                    otobus_id UUID := gen_random_uuid();
                    scania_id UUID := gen_random_uuid();
                    volvo_id UUID := gen_random_uuid();
                    mercedes_id UUID := gen_random_uuid();
                    man_id UUID := gen_random_uuid();
                    daf_id UUID := gen_random_uuid();
                    hidrolik_pompa_id UUID;
                    disli_pompa_id UUID := gen_random_uuid();
                    pistonlu_pompa_id UUID := gen_random_uuid();
                    paletli_pompa_id UUID := gen_random_uuid();
                    plc_id UUID;
                    siemens_id UUID := gen_random_uuid();
                    omron_id UUID := gen_random_uuid();
                    allen_id UUID := gen_random_uuid();
                    mitsubishi_id UUID := gen_random_uuid();
                BEGIN
                    -- Motor Yedek Parçaları ID'sini bul
                    SELECT ""Id"" INTO motor_yedek_id FROM ""Categories"" WHERE ""Name"" = 'Motor Yedek Parçaları' AND ""IsActive"" = true LIMIT 1;
                    
                    IF motor_yedek_id IS NOT NULL THEN
                        -- Çekici zaten var mı kontrol et
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Çekici' AND ""ParentId"" = motor_yedek_id) THEN
                            -- 3. seviye - Araç Tipleri
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"") VALUES
                            (cekici_id, 'Çekici', 'Çekici motor yedek parçaları', true, motor_yedek_id, NOW()),
                            (kamyon_id, 'Kamyon', 'Kamyon motor yedek parçaları', true, motor_yedek_id, NOW()),
                            (otobus_id, 'Otobüs', 'Otobüs motor yedek parçaları', true, motor_yedek_id, NOW());
                            
                            -- 4. seviye - Çekici Markaları
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"") VALUES
                            (scania_id, 'Scania', 'Scania çekici yedek parçaları', true, cekici_id, NOW()),
                            (volvo_id, 'Volvo', 'Volvo çekici yedek parçaları', true, cekici_id, NOW()),
                            (mercedes_id, 'Mercedes', 'Mercedes çekici yedek parçaları', true, cekici_id, NOW()),
                            (man_id, 'MAN', 'MAN çekici yedek parçaları', true, cekici_id, NOW()),
                            (daf_id, 'DAF', 'DAF çekici yedek parçaları', true, cekici_id, NOW());
                            
                            -- 5. seviye - Scania Modelleri
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"") VALUES
                            (gen_random_uuid(), 'R410', 'Scania R410 yedek parçaları', true, scania_id, NOW()),
                            (gen_random_uuid(), 'R450', 'Scania R450 yedek parçaları', true, scania_id, NOW()),
                            (gen_random_uuid(), 'R500', 'Scania R500 yedek parçaları', true, scania_id, NOW()),
                            (gen_random_uuid(), 'S500', 'Scania S500 yedek parçaları', true, scania_id, NOW());
                            
                            -- 5. seviye - Volvo Modelleri
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"") VALUES
                            (gen_random_uuid(), 'FH16', 'Volvo FH16 yedek parçaları', true, volvo_id, NOW()),
                            (gen_random_uuid(), 'FH500', 'Volvo FH500 yedek parçaları', true, volvo_id, NOW()),
                            (gen_random_uuid(), 'FM', 'Volvo FM yedek parçaları', true, volvo_id, NOW());
                            
                            -- 5. seviye - Mercedes Modelleri
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"") VALUES
                            (gen_random_uuid(), 'Actros', 'Mercedes Actros yedek parçaları', true, mercedes_id, NOW()),
                            (gen_random_uuid(), 'Arocs', 'Mercedes Arocs yedek parçaları', true, mercedes_id, NOW()),
                            (gen_random_uuid(), 'Axor', 'Mercedes Axor yedek parçaları', true, mercedes_id, NOW());
                        END IF;
                    END IF;
                    
                    -- Hidrolik Pompalar ID'sini bul
                    SELECT ""Id"" INTO hidrolik_pompa_id FROM ""Categories"" WHERE ""Name"" = 'Hidrolik Pompalar' AND ""IsActive"" = true LIMIT 1;
                    
                    IF hidrolik_pompa_id IS NOT NULL THEN
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Dişli Pompalar' AND ""ParentId"" = hidrolik_pompa_id) THEN
                            -- 3. seviye - Pompa Tipleri
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"") VALUES
                            (disli_pompa_id, 'Dişli Pompalar', 'Dişli tip hidrolik pompalar', true, hidrolik_pompa_id, NOW()),
                            (pistonlu_pompa_id, 'Pistonlu Pompalar', 'Pistonlu tip hidrolik pompalar', true, hidrolik_pompa_id, NOW()),
                            (paletli_pompa_id, 'Paletli Pompalar', 'Paletli tip hidrolik pompalar', true, hidrolik_pompa_id, NOW());
                            
                            -- 4. seviye - Markalar
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"") VALUES
                            (gen_random_uuid(), 'Bosch Rexroth', 'Bosch Rexroth dişli pompalar', true, disli_pompa_id, NOW()),
                            (gen_random_uuid(), 'Parker', 'Parker dişli pompalar', true, disli_pompa_id, NOW()),
                            (gen_random_uuid(), 'Casappa', 'Casappa dişli pompalar', true, disli_pompa_id, NOW());
                        END IF;
                    END IF;
                    
                    -- PLC ve Kontrol ID'sini bul
                    SELECT ""Id"" INTO plc_id FROM ""Categories"" WHERE ""Name"" = 'PLC ve Kontrol' AND ""IsActive"" = true LIMIT 1;
                    
                    IF plc_id IS NOT NULL THEN
                        IF NOT EXISTS (SELECT 1 FROM ""Categories"" WHERE ""Name"" = 'Siemens' AND ""ParentId"" = plc_id) THEN
                            -- 3. seviye - PLC Markaları
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"") VALUES
                            (siemens_id, 'Siemens', 'Siemens PLC sistemleri', true, plc_id, NOW()),
                            (omron_id, 'Omron', 'Omron PLC sistemleri', true, plc_id, NOW()),
                            (allen_id, 'Allen Bradley', 'Allen Bradley PLC sistemleri', true, plc_id, NOW()),
                            (mitsubishi_id, 'Mitsubishi', 'Mitsubishi PLC sistemleri', true, plc_id, NOW());
                            
                            -- 4. seviye - Siemens Serileri
                            INSERT INTO ""Categories"" (""Id"", ""Name"", ""Description"", ""IsActive"", ""ParentId"", ""CreatedDate"") VALUES
                            (gen_random_uuid(), 'S7-1200', 'Siemens S7-1200 serisi', true, siemens_id, NOW()),
                            (gen_random_uuid(), 'S7-1500', 'Siemens S7-1500 serisi', true, siemens_id, NOW()),
                            (gen_random_uuid(), 'S7-300', 'Siemens S7-300 serisi', true, siemens_id, NOW());
                        END IF;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eklenen kategorileri sil (5. seviyeden başlayarak geriye doğru)
            migrationBuilder.Sql(@"
                -- Scania modelleri
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('R410', 'R450', 'R500', 'S500') 
                    AND ""ParentId"" IN (SELECT ""Id"" FROM ""Categories"" WHERE ""Name"" = 'Scania');
                
                -- Volvo modelleri
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('FH16', 'FH500', 'FM') 
                    AND ""ParentId"" IN (SELECT ""Id"" FROM ""Categories"" WHERE ""Name"" = 'Volvo');
                
                -- Mercedes modelleri
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('Actros', 'Arocs', 'Axor') 
                    AND ""ParentId"" IN (SELECT ""Id"" FROM ""Categories"" WHERE ""Name"" = 'Mercedes');
                
                -- Çekici markaları
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('Scania', 'Volvo', 'Mercedes', 'MAN', 'DAF') 
                    AND ""ParentId"" IN (SELECT ""Id"" FROM ""Categories"" WHERE ""Name"" = 'Çekici');
                
                -- Araç tipleri
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('Çekici', 'Kamyon', 'Otobüs') 
                    AND ""ParentId"" IN (SELECT ""Id"" FROM ""Categories"" WHERE ""Name"" = 'Motor Yedek Parçaları');
                
                -- Pompa markaları
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('Bosch Rexroth', 'Parker', 'Casappa') 
                    AND ""ParentId"" IN (SELECT ""Id"" FROM ""Categories"" WHERE ""Name"" = 'Dişli Pompalar');
                
                -- Pompa tipleri
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('Dişli Pompalar', 'Pistonlu Pompalar', 'Paletli Pompalar') 
                    AND ""ParentId"" IN (SELECT ""Id"" FROM ""Categories"" WHERE ""Name"" = 'Hidrolik Pompalar');
                
                -- Siemens serileri
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('S7-1200', 'S7-1500', 'S7-300') 
                    AND ""ParentId"" IN (SELECT ""Id"" FROM ""Categories"" WHERE ""Name"" = 'Siemens');
                
                -- PLC markaları
                DELETE FROM ""Categories"" WHERE ""Name"" IN ('Siemens', 'Omron', 'Allen Bradley', 'Mitsubishi') 
                    AND ""ParentId"" IN (SELECT ""Id"" FROM ""Categories"" WHERE ""Name"" = 'PLC ve Kontrol');
            ");
        }
    }
}
