using System;
using System.Collections.Generic;
using EPS_Web_App.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AstRecord> AstRecords { get; set; }

    public virtual DbSet<DataEntryAudit> DataEntryAudits { get; set; }

    public virtual DbSet<FormVersion> FormVersions { get; set; }

    public virtual DbSet<LookupValue> LookupValues { get; set; }

    public virtual DbSet<QuestionnaireEntry> QuestionnaireEntries { get; set; }

    public virtual DbSet<ReconciliationAlert> ReconciliationAlerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AstRecord>(entity =>
        {
            entity.ToTable("ast_record");

            entity.HasIndex(e => e.Lan, "IX_ast_lan");

            entity.HasIndex(e => e.SiteCode, "IX_ast_site_code");

            entity.HasIndex(e => e.SpecimenId, "IX_ast_specimen_id");

            entity.Property(e => e.AstRecordId).HasColumnName("ast_record_id");
            entity.Property(e => e.AmikacinInt)
                .HasMaxLength(100)
                .HasColumnName("amikacin_int");
            entity.Property(e => e.AmikacinMic)
                .HasMaxLength(100)
                .HasColumnName("amikacin_mic");
            entity.Property(e => e.AmoxicillinClavulanateInt)
                .HasMaxLength(100)
                .HasColumnName("amoxicillin_clavulanate_int");
            entity.Property(e => e.AmoxicillinClavulanateMic)
                .HasMaxLength(100)
                .HasColumnName("amoxicillin_clavulanate_mic");
            entity.Property(e => e.AmpicillinInt)
                .HasMaxLength(100)
                .HasColumnName("ampicillin_int");
            entity.Property(e => e.AmpicillinMic)
                .HasMaxLength(100)
                .HasColumnName("ampicillin_mic");
            entity.Property(e => e.AmpicillinSulbactamInt)
                .HasMaxLength(100)
                .HasColumnName("ampicillin_sulbactam_int");
            entity.Property(e => e.AmpicillinSulbactamMic)
                .HasMaxLength(100)
                .HasColumnName("ampicillin_sulbactam_mic");
            entity.Property(e => e.AzithromycinInt)
                .HasMaxLength(100)
                .HasColumnName("azithromycin_int");
            entity.Property(e => e.AzithromycinMic)
                .HasMaxLength(100)
                .HasColumnName("azithromycin_mic");
            entity.Property(e => e.AztreonamInt)
                .HasMaxLength(100)
                .HasColumnName("aztreonam_int");
            entity.Property(e => e.AztreonamMic)
                .HasMaxLength(100)
                .HasColumnName("aztreonam_mic");
            entity.Property(e => e.BacterialIdentification).HasColumnName("bacterial_identification");
            entity.Property(e => e.CefazolinInt)
                .HasMaxLength(100)
                .HasColumnName("cefazolin_int");
            entity.Property(e => e.CefazolinMic)
                .HasMaxLength(100)
                .HasColumnName("cefazolin_mic");
            entity.Property(e => e.CefepimeInt)
                .HasMaxLength(100)
                .HasColumnName("cefepime_int");
            entity.Property(e => e.CefepimeMic)
                .HasMaxLength(100)
                .HasColumnName("cefepime_mic");
            entity.Property(e => e.CefotaximeClavulanateInt)
                .HasMaxLength(100)
                .HasColumnName("cefotaxime_clavulanate_int");
            entity.Property(e => e.CefotaximeClavulanateMic)
                .HasMaxLength(100)
                .HasColumnName("cefotaxime_clavulanate_mic");
            entity.Property(e => e.CefotaximeInt)
                .HasMaxLength(100)
                .HasColumnName("cefotaxime_int");
            entity.Property(e => e.CefotaximeMic)
                .HasMaxLength(100)
                .HasColumnName("cefotaxime_mic");
            entity.Property(e => e.CefotetanInt)
                .HasMaxLength(100)
                .HasColumnName("cefotetan_int");
            entity.Property(e => e.CefotetanMic)
                .HasMaxLength(100)
                .HasColumnName("cefotetan_mic");
            entity.Property(e => e.CefoxitinInt)
                .HasMaxLength(100)
                .HasColumnName("cefoxitin_int");
            entity.Property(e => e.CefoxitinMic)
                .HasMaxLength(100)
                .HasColumnName("cefoxitin_mic");
            entity.Property(e => e.CeftazidimeClavulanateInt)
                .HasMaxLength(100)
                .HasColumnName("ceftazidime_clavulanate_int");
            entity.Property(e => e.CeftazidimeClavulanateMic)
                .HasMaxLength(100)
                .HasColumnName("ceftazidime_clavulanate_mic");
            entity.Property(e => e.CeftazidimeInt)
                .HasMaxLength(100)
                .HasColumnName("ceftazidime_int");
            entity.Property(e => e.CeftazidimeMic)
                .HasMaxLength(100)
                .HasColumnName("ceftazidime_mic");
            entity.Property(e => e.CeftolozaneTazobactamInt)
                .HasMaxLength(100)
                .HasColumnName("ceftolozane_tazobactam_int");
            entity.Property(e => e.CeftolozaneTazobactamMic)
                .HasMaxLength(100)
                .HasColumnName("ceftolozane_tazobactam_mic");
            entity.Property(e => e.CeftriaxoneInt)
                .HasMaxLength(100)
                .HasColumnName("ceftriaxone_int");
            entity.Property(e => e.CeftriaxoneMic)
                .HasMaxLength(100)
                .HasColumnName("ceftriaxone_mic");
            entity.Property(e => e.CefuroximeInt)
                .HasMaxLength(100)
                .HasColumnName("cefuroxime_int");
            entity.Property(e => e.CefuroximeMic)
                .HasMaxLength(100)
                .HasColumnName("cefuroxime_mic");
            entity.Property(e => e.CephalothinInt)
                .HasMaxLength(100)
                .HasColumnName("cephalothin_int");
            entity.Property(e => e.CephalothinMic)
                .HasMaxLength(100)
                .HasColumnName("cephalothin_mic");
            entity.Property(e => e.ChloramphenicolInt)
                .HasMaxLength(100)
                .HasColumnName("chloramphenicol_int");
            entity.Property(e => e.ChloramphenicolMic)
                .HasMaxLength(100)
                .HasColumnName("chloramphenicol_mic");
            entity.Property(e => e.CiprofloxacinInt)
                .HasMaxLength(100)
                .HasColumnName("ciprofloxacin_int");
            entity.Property(e => e.CiprofloxacinMic)
                .HasMaxLength(100)
                .HasColumnName("ciprofloxacin_mic");
            entity.Property(e => e.ColistinInt)
                .HasMaxLength(100)
                .HasColumnName("colistin_int");
            entity.Property(e => e.ColistinMic)
                .HasMaxLength(100)
                .HasColumnName("colistin_mic");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_ast_created_at")
                .HasColumnName("created_at");
            entity.Property(e => e.DiagnosticTest).HasColumnName("diagnostic_test");
            entity.Property(e => e.DiarrheaMedicationPast72h)
                .HasMaxLength(50)
                .HasColumnName("diarrhea_medication_past_72h");
            entity.Property(e => e.ErtapenemInt)
                .HasMaxLength(100)
                .HasColumnName("ertapenem_int");
            entity.Property(e => e.ErtapenemMic)
                .HasMaxLength(100)
                .HasColumnName("ertapenem_mic");
            entity.Property(e => e.EsblStatus)
                .HasMaxLength(50)
                .HasColumnName("esbl_status");
            entity.Property(e => e.FosfomycinInt)
                .HasMaxLength(100)
                .HasColumnName("fosfomycin_int");
            entity.Property(e => e.FosfomycinMic)
                .HasMaxLength(100)
                .HasColumnName("fosfomycin_mic");
            entity.Property(e => e.GatifloxacinInt)
                .HasMaxLength(100)
                .HasColumnName("gatifloxacin_int");
            entity.Property(e => e.GatifloxacinMic)
                .HasMaxLength(100)
                .HasColumnName("gatifloxacin_mic");
            entity.Property(e => e.GentamicinInt)
                .HasMaxLength(100)
                .HasColumnName("gentamicin_int");
            entity.Property(e => e.GentamicinMic)
                .HasMaxLength(100)
                .HasColumnName("gentamicin_mic");
            entity.Property(e => e.GtdStatus)
                .HasMaxLength(50)
                .HasColumnName("gtd_status");
            entity.Property(e => e.IllnessFunctionalImpact).HasColumnName("illness_functional_impact");
            entity.Property(e => e.ImipenemInt)
                .HasMaxLength(100)
                .HasColumnName("imipenem_int");
            entity.Property(e => e.ImipenemMic)
                .HasMaxLength(100)
                .HasColumnName("imipenem_mic");
            entity.Property(e => e.Lan)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("lan");
            entity.Property(e => e.LevofloxacinInt)
                .HasMaxLength(100)
                .HasColumnName("levofloxacin_int");
            entity.Property(e => e.LevofloxacinMic)
                .HasMaxLength(100)
                .HasColumnName("levofloxacin_mic");
            entity.Property(e => e.LinkageStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Unlinked", "DF_ast_linkage_status")
                .HasColumnName("linkage_status");
            entity.Property(e => e.MdrStatus)
                .HasMaxLength(50)
                .HasColumnName("mdr_status");
            entity.Property(e => e.MedicationsPast72h).HasColumnName("medications_past_72h");
            entity.Property(e => e.MeropenemInt)
                .HasMaxLength(100)
                .HasColumnName("meropenem_int");
            entity.Property(e => e.MeropenemMic)
                .HasMaxLength(100)
                .HasColumnName("meropenem_mic");
            entity.Property(e => e.MonthCollected)
                .HasMaxLength(100)
                .HasColumnName("month_collected");
            entity.Property(e => e.MoxifloxacinInt)
                .HasMaxLength(100)
                .HasColumnName("moxifloxacin_int");
            entity.Property(e => e.MoxifloxacinMic)
                .HasMaxLength(100)
                .HasColumnName("moxifloxacin_mic");
            entity.Property(e => e.NalidixicAcidInt)
                .HasMaxLength(100)
                .HasColumnName("nalidixic_acid_int");
            entity.Property(e => e.NalidixicAcidMic)
                .HasMaxLength(100)
                .HasColumnName("nalidixic_acid_mic");
            entity.Property(e => e.NitrofurantoinInt)
                .HasMaxLength(100)
                .HasColumnName("nitrofurantoin_int");
            entity.Property(e => e.NitrofurantoinMic)
                .HasMaxLength(100)
                .HasColumnName("nitrofurantoin_mic");
            entity.Property(e => e.NorfloxacinInt)
                .HasMaxLength(100)
                .HasColumnName("norfloxacin_int");
            entity.Property(e => e.NorfloxacinMic)
                .HasMaxLength(100)
                .HasColumnName("norfloxacin_mic");
            entity.Property(e => e.ParasiteIdentification).HasColumnName("parasite_identification");
            entity.Property(e => e.ParticipantType)
                .HasMaxLength(50)
                .HasColumnName("participant_type");
            entity.Property(e => e.PiperacillinInt)
                .HasMaxLength(100)
                .HasColumnName("piperacillin_int");
            entity.Property(e => e.PiperacillinMic)
                .HasMaxLength(100)
                .HasColumnName("piperacillin_mic");
            entity.Property(e => e.PiperacillinTazobactamInt)
                .HasMaxLength(100)
                .HasColumnName("piperacillin_tazobactam_int");
            entity.Property(e => e.PiperacillinTazobactamMic)
                .HasMaxLength(100)
                .HasColumnName("piperacillin_tazobactam_mic");
            entity.Property(e => e.RifampicinInt)
                .HasMaxLength(100)
                .HasColumnName("rifampicin_int");
            entity.Property(e => e.RifampicinMic)
                .HasMaxLength(100)
                .HasColumnName("rifampicin_mic");
            entity.Property(e => e.SiteCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("site_code");
            entity.Property(e => e.SpecimenId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("specimen_id");
            entity.Property(e => e.TetracyclineInt)
                .HasMaxLength(100)
                .HasColumnName("tetracycline_int");
            entity.Property(e => e.TetracyclineMic)
                .HasMaxLength(100)
                .HasColumnName("tetracycline_mic");
            entity.Property(e => e.TicarcillinClavulanateInt)
                .HasMaxLength(100)
                .HasColumnName("ticarcillin_clavulanate_int");
            entity.Property(e => e.TicarcillinClavulanateMic)
                .HasMaxLength(100)
                .HasColumnName("ticarcillin_clavulanate_mic");
            entity.Property(e => e.TigecyclineInt)
                .HasMaxLength(100)
                .HasColumnName("tigecycline_int");
            entity.Property(e => e.TigecyclineMic)
                .HasMaxLength(100)
                .HasColumnName("tigecycline_mic");
            entity.Property(e => e.TobramycinInt)
                .HasMaxLength(100)
                .HasColumnName("tobramycin_int");
            entity.Property(e => e.TobramycinMic)
                .HasMaxLength(100)
                .HasColumnName("tobramycin_mic");
            entity.Property(e => e.TrimethoprimSulfamethoxazoleInt)
                .HasMaxLength(100)
                .HasColumnName("trimethoprim_sulfamethoxazole_int");
            entity.Property(e => e.TrimethoprimSulfamethoxazoleMic)
                .HasMaxLength(100)
                .HasColumnName("trimethoprim_sulfamethoxazole_mic");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasColumnName("updated_at");
            entity.Property(e => e.ViralIdentification).HasColumnName("viral_identification");
        });

        modelBuilder.Entity<DataEntryAudit>(entity =>
        {
            entity.HasKey(e => e.AuditId);

            entity.ToTable("data_entry_audit");

            entity.HasIndex(e => new { e.RecordType, e.RecordKey }, "IX_audit_record_key");

            entity.Property(e => e.AuditId).HasColumnName("audit_id");
            entity.Property(e => e.ChangedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_audit_changed_at")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy)
                .HasMaxLength(255)
                .HasColumnName("changed_by");
            entity.Property(e => e.FieldName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("field_name");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RecordKey)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("record_key");
            entity.Property(e => e.RecordType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("record_type");
        });

        modelBuilder.Entity<FormVersion>(entity =>
        {
            entity.ToTable("form_version");

            entity.HasIndex(e => new { e.FormName, e.VersionNumber }, "UQ_form_version").IsUnique();

            entity.Property(e => e.FormVersionId).HasColumnName("form_version_id");
            entity.Property(e => e.EffectiveFrom)
                .HasPrecision(0)
                .HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo)
                .HasPrecision(0)
                .HasColumnName("effective_to");
            entity.Property(e => e.FormName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("form_name");
            entity.Property(e => e.IsCurrent)
                .HasDefaultValue(true, "DF_form_version_current")
                .HasColumnName("is_current");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.VersionNumber)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("version_number");
        });

        modelBuilder.Entity<LookupValue>(entity =>
        {
            entity.HasKey(e => e.LookupId);

            entity.ToTable("lookup_value");

            entity.HasIndex(e => new { e.LookupGroup, e.LookupCode }, "UQ_lookup_group_code").IsUnique();

            entity.Property(e => e.LookupId).HasColumnName("lookup_id");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true, "DF_lookup_is_active")
                .HasColumnName("is_active");
            entity.Property(e => e.LookupCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("lookup_code");
            entity.Property(e => e.LookupGroup)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("lookup_group");
            entity.Property(e => e.LookupLabel)
                .HasMaxLength(255)
                .HasColumnName("lookup_label");
        });

        modelBuilder.Entity<QuestionnaireEntry>(entity =>
        {
            entity.HasKey(e => e.QuestionnaireId);

            entity.ToTable("questionnaire_entry");

            entity.HasIndex(e => e.Lan, "IX_questionnaire_lan");

            entity.HasIndex(e => e.SiteCode, "IX_questionnaire_site_code");

            entity.HasIndex(e => e.SpecimenId, "UQ_questionnaire_specimen_id").IsUnique();

            entity.Property(e => e.QuestionnaireId).HasColumnName("questionnaire_id");
            entity.Property(e => e.AbdominalCramps).HasColumnName("abdominal_cramps");
            entity.Property(e => e.AbdominalCrampsDuration).HasColumnName("abdominal_cramps_duration");
            entity.Property(e => e.AbdominalCrampsSeverity).HasColumnName("abdominal_cramps_severity");
            entity.Property(e => e.AdditionalSymptoms).HasColumnName("additional_symptoms");
            entity.Property(e => e.AdditionalSymptomsSeverity).HasColumnName("additional_symptoms_severity");
            entity.Property(e => e.AdmissionDate).HasColumnName("admission_date");
            entity.Property(e => e.AdmissionTime).HasColumnName("admission_time");
            entity.Property(e => e.Admitted).HasColumnName("admitted");
            entity.Property(e => e.AdultConsciousnessResponse).HasColumnName("adult_consciousness_response");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.BacterialIdentification).HasColumnName("bacterial_identification");
            entity.Property(e => e.BloodPressure).HasColumnName("blood_pressure");
            entity.Property(e => e.BloodyStool).HasColumnName("bloody_stool");
            entity.Property(e => e.BloodyStoolDuration).HasColumnName("bloody_stool_duration");
            entity.Property(e => e.BodyTemperature).HasColumnName("body_temperature");
            entity.Property(e => e.BodyWeight).HasColumnName("body_weight");
            entity.Property(e => e.CapillaryRefill).HasColumnName("capillary_refill");
            entity.Property(e => e.ChildAssessment).HasColumnName("child_assessment");
            entity.Property(e => e.ChildConsciousnessResponse).HasColumnName("child_consciousness_response");
            entity.Property(e => e.ChildDrinkingBreastfeeding).HasColumnName("child_drinking_breastfeeding");
            entity.Property(e => e.ChildRestlessness).HasColumnName("child_restlessness");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_questionnaire_created_at")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DiarrheaDuration).HasColumnName("diarrhea_duration");
            entity.Property(e => e.DischargeDeathDiagnosis).HasColumnName("discharge_death_diagnosis");
            entity.Property(e => e.Disposition).HasColumnName("disposition");
            entity.Property(e => e.DispositionDate).HasColumnName("disposition_date");
            entity.Property(e => e.DispositionTime).HasColumnName("disposition_time");
            entity.Property(e => e.ExcessiveGas).HasColumnName("excessive_gas");
            entity.Property(e => e.ExcessiveGasDuration).HasColumnName("excessive_gas_duration");
            entity.Property(e => e.ExcessiveGasSeverity).HasColumnName("excessive_gas_severity");
            entity.Property(e => e.FecalIncontinence).HasColumnName("fecal_incontinence");
            entity.Property(e => e.FecalIncontinenceDuration).HasColumnName("fecal_incontinence_duration");
            entity.Property(e => e.FecalIncontinenceSeverity).HasColumnName("fecal_incontinence_severity");
            entity.Property(e => e.Fever).HasColumnName("fever");
            entity.Property(e => e.FeverDuration).HasColumnName("fever_duration");
            entity.Property(e => e.FeverSeverity).HasColumnName("fever_severity");
            entity.Property(e => e.FirstSymptom).HasColumnName("first_symptom");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.GeneralCondition).HasColumnName("general_condition");
            entity.Property(e => e.Headache).HasColumnName("headache");
            entity.Property(e => e.HeadacheDuration).HasColumnName("headache_duration");
            entity.Property(e => e.HeadacheSeverity).HasColumnName("headache_severity");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.HivStatus).HasColumnName("hiv_status");
            entity.Property(e => e.IllnessFunctionalImpact).HasColumnName("illness_functional_impact");
            entity.Property(e => e.IsolationDate).HasColumnName("isolation_date");
            entity.Property(e => e.IvRehydration).HasColumnName("iv_rehydration");
            entity.Property(e => e.JointAches).HasColumnName("joint_aches");
            entity.Property(e => e.JointAchesDuration).HasColumnName("joint_aches_duration");
            entity.Property(e => e.JointAchesSeverity).HasColumnName("joint_aches_severity");
            entity.Property(e => e.Lan)
                .HasMaxLength(50)
                .HasColumnName("lan");
            entity.Property(e => e.Lightheadedness).HasColumnName("lightheadedness");
            entity.Property(e => e.LightheadednessDuration).HasColumnName("lightheadedness_duration");
            entity.Property(e => e.LightheadednessSeverity).HasColumnName("lightheadedness_severity");
            entity.Property(e => e.LooseStools24h).HasColumnName("loose_stools_24h");
            entity.Property(e => e.LooseStools8h).HasColumnName("loose_stools_8h");
            entity.Property(e => e.LossOfAppetite).HasColumnName("loss_of_appetite");
            entity.Property(e => e.LossOfAppetiteDuration).HasColumnName("loss_of_appetite_duration");
            entity.Property(e => e.MainJob).HasColumnName("main_job");
            entity.Property(e => e.MalaiseFatigue).HasColumnName("malaise_fatigue");
            entity.Property(e => e.MalaiseFatigueDuration).HasColumnName("malaise_fatigue_duration");
            entity.Property(e => e.MalaiseFatigueSeverity).HasColumnName("malaise_fatigue_severity");
            entity.Property(e => e.MalnutritionStatus).HasColumnName("malnutrition_status");
            entity.Property(e => e.MaxLooseStools24h).HasColumnName("max_loose_stools_24h");
            entity.Property(e => e.Muac).HasColumnName("muac");
            entity.Property(e => e.MucousStool).HasColumnName("mucous_stool");
            entity.Property(e => e.MuscleAches).HasColumnName("muscle_aches");
            entity.Property(e => e.MuscleAchesDuration).HasColumnName("muscle_aches_duration");
            entity.Property(e => e.MuscleAchesSeverity).HasColumnName("muscle_aches_severity");
            entity.Property(e => e.Nausea).HasColumnName("nausea");
            entity.Property(e => e.NauseaDuration).HasColumnName("nausea_duration");
            entity.Property(e => e.NauseaSeverity).HasColumnName("nausea_severity");
            entity.Property(e => e.Occupation).HasColumnName("occupation");
            entity.Property(e => e.OralRehydration).HasColumnName("oral_rehydration");
            entity.Property(e => e.OtherRotavirusVaccineDate).HasColumnName("other_rotavirus_vaccine_date");
            entity.Property(e => e.OtherRotavirusVaccineStatus).HasColumnName("other_rotavirus_vaccine_status");
            entity.Property(e => e.OtherSymptoms).HasColumnName("other_symptoms");
            entity.Property(e => e.OtherSymptomsSeverity).HasColumnName("other_symptoms_severity");
            entity.Property(e => e.OtherSymptomsSpecified).HasColumnName("other_symptoms_specified");
            entity.Property(e => e.OtherTreatment).HasColumnName("other_treatment");
            entity.Property(e => e.OutpatientTreatment).HasColumnName("outpatient_treatment");
            entity.Property(e => e.PainfulStrainingDuration).HasColumnName("painful_straining_duration");
            entity.Property(e => e.PainfulStrainingSeverity).HasColumnName("painful_straining_severity");
            entity.Property(e => e.PainfulStrainingStool).HasColumnName("painful_straining_stool");
            entity.Property(e => e.ParasiteIdentification).HasColumnName("parasite_identification");
            entity.Property(e => e.PatientClinicalState).HasColumnName("patient_clinical_state");
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.Residence).HasColumnName("residence");
            entity.Property(e => e.RespiratoryRate).HasColumnName("respiratory_rate");
            entity.Property(e => e.RiceWaterStool).HasColumnName("rice_water_stool");
            entity.Property(e => e.RotavirusDose1Date).HasColumnName("rotavirus_dose_1_date");
            entity.Property(e => e.RotavirusDose1Status).HasColumnName("rotavirus_dose_1_status");
            entity.Property(e => e.RotavirusDose2Date).HasColumnName("rotavirus_dose_2_date");
            entity.Property(e => e.RotavirusDose2Status).HasColumnName("rotavirus_dose_2_status");
            entity.Property(e => e.SiteCode)
                .HasMaxLength(50)
                .HasColumnName("site_code");
            entity.Property(e => e.SkinPinchBack).HasColumnName("skin_pinch_back");
            entity.Property(e => e.SpecimenId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("specimen_id");
            entity.Property(e => e.StoolUrgency).HasColumnName("stool_urgency");
            entity.Property(e => e.StoolUrgencyDuration).HasColumnName("stool_urgency_duration");
            entity.Property(e => e.StoolUrgencySeverity).HasColumnName("stool_urgency_severity");
            entity.Property(e => e.SymptomOnsetDate).HasColumnName("symptom_onset_date");
            entity.Property(e => e.SymptomOnsetTime).HasColumnName("symptom_onset_time");
            entity.Property(e => e.TreatmentGiven).HasColumnName("treatment_given");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasColumnName("updated_at");
            entity.Property(e => e.ViralIdentification).HasColumnName("viral_identification");
            entity.Property(e => e.Vomiting).HasColumnName("vomiting");
            entity.Property(e => e.VomitingCount).HasColumnName("vomiting_count");
            entity.Property(e => e.VomitingDuration).HasColumnName("vomiting_duration");
            entity.Property(e => e.VomitingSeverity).HasColumnName("vomiting_severity");
            entity.Property(e => e.WaterSourceType).HasColumnName("water_source_type");
            entity.Property(e => e.WaterTreatment).HasColumnName("water_treatment");
        });

        modelBuilder.Entity<ReconciliationAlert>(entity =>
        {
            entity.HasKey(e => e.AlertId);

            entity.ToTable("reconciliation_alert");

            entity.HasIndex(e => e.SpecimenId, "IX_reconciliation_specimen_id");

            entity.HasIndex(e => e.Status, "IX_reconciliation_status");

            entity.Property(e => e.AlertId).HasColumnName("alert_id");
            entity.Property(e => e.AlertType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("alert_type");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_reconciliation_created_at")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Medium", "DF_reconciliation_priority")
                .HasColumnName("priority");
            entity.Property(e => e.ResolutionNote).HasColumnName("resolution_note");
            entity.Property(e => e.ResolvedAt)
                .HasPrecision(0)
                .HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedBy)
                .HasMaxLength(255)
                .HasColumnName("resolved_by");
            entity.Property(e => e.SourceRecord)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("source_record");
            entity.Property(e => e.SpecimenId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("specimen_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Open", "DF_reconciliation_status")
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
