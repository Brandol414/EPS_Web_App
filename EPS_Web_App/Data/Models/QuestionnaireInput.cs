namespace EPS_Web_App.Data.Models;

public sealed class QuestionnaireInput
{
	// =========================================================
	// 01 — RECORD IDENTITY
	// =========================================================

	public string? SiteCode { get; set; }

	public string? SpecimenId { get; set; }

	public string? Lan { get; set; }

	public string? IsolationDate { get; set; }


	// =========================================================
	// 02 — PARTICIPANT
	// =========================================================

	public string? Age { get; set; }

	public string? DateOfBirth { get; set; }

	public string? Gender { get; set; }

	public string? Residence { get; set; }

	public string? Occupation { get; set; }

	public string? Rank { get; set; }

	public string? MainJob { get; set; }


	// =========================================================
	// 03 — ROTAVIRUS / HIV
	// =========================================================

	public string? RotavirusDose1Status { get; set; }

	public string? RotavirusDose1Date { get; set; }

	public string? RotavirusDose2Status { get; set; }

	public string? RotavirusDose2Date { get; set; }

	public string? OtherRotavirusVaccineStatus { get; set; }

	public string? OtherRotavirusVaccineDate { get; set; }

	public string? HivStatus { get; set; }


	// =========================================================
	// 04 — SYMPTOM ONSET / DIARRHOEA
	// =========================================================

	public string? FirstSymptom { get; set; }

	public string? SymptomOnsetDate { get; set; }

	public string? SymptomOnsetTime { get; set; }

	public string? DiarrheaDuration { get; set; }

	public string? MaxLooseStools24h { get; set; }

	public string? LooseStools8h { get; set; }

	public string? LooseStools24h { get; set; }

	public string? MucousStool { get; set; }

	public string? BloodyStool { get; set; }

	public string? BloodyStoolDuration { get; set; }

	public string? RiceWaterStool { get; set; }


	// =========================================================
	// 05 — ASSOCIATED SYMPTOMS
	// =========================================================

	public string? AbdominalCramps { get; set; }

	public string? AbdominalCrampsDuration { get; set; }

	public string? AbdominalCrampsSeverity { get; set; }

	public string? ExcessiveGas { get; set; }

	public string? ExcessiveGasDuration { get; set; }

	public string? ExcessiveGasSeverity { get; set; }

	public string? Nausea { get; set; }

	public string? NauseaDuration { get; set; }

	public string? NauseaSeverity { get; set; }

	public string? Fever { get; set; }

	public string? FeverDuration { get; set; }

	public string? FeverSeverity { get; set; }

	public string? PainfulStrainingStool { get; set; }

	public string? PainfulStrainingDuration { get; set; }

	public string? PainfulStrainingSeverity { get; set; }

	public string? MalaiseFatigue { get; set; }

	public string? MalaiseFatigueDuration { get; set; }

	public string? MalaiseFatigueSeverity { get; set; }

	public string? Vomiting { get; set; }

	public string? VomitingDuration { get; set; }

	public string? VomitingCount { get; set; }

	public string? VomitingSeverity { get; set; }

	public string? Headache { get; set; }

	public string? HeadacheDuration { get; set; }

	public string? HeadacheSeverity { get; set; }

	public string? LossOfAppetite { get; set; }

	public string? LossOfAppetiteDuration { get; set; }

	public string? Lightheadedness { get; set; }

	public string? LightheadednessDuration { get; set; }

	public string? LightheadednessSeverity { get; set; }

	public string? StoolUrgency { get; set; }

	public string? StoolUrgencyDuration { get; set; }

	public string? StoolUrgencySeverity { get; set; }

	public string? MuscleAches { get; set; }

	public string? MuscleAchesDuration { get; set; }

	public string? MuscleAchesSeverity { get; set; }

	public string? JointAches { get; set; }

	public string? JointAchesDuration { get; set; }

	public string? JointAchesSeverity { get; set; }

	public string? FecalIncontinence { get; set; }

	public string? FecalIncontinenceDuration { get; set; }

	public string? FecalIncontinenceSeverity { get; set; }

	public string? OtherSymptoms { get; set; }

	public string? OtherSymptomsSpecified { get; set; }

	public string? OtherSymptomsSeverity { get; set; }

	public string? AdditionalSymptoms { get; set; }

	public string? AdditionalSymptomsSeverity { get; set; }


	// =========================================================
	// 06 — CLINICAL ASSESSMENT
	// =========================================================

	public string? GeneralCondition { get; set; }

	public string? BodyTemperature { get; set; }

	public string? BodyWeight { get; set; }

	public string? Height { get; set; }

	public string? BloodPressure { get; set; }

	public string? RespiratoryRate { get; set; }

	public string? MalnutritionStatus { get; set; }

	// Child assessment

	public string? ChildConsciousnessResponse { get; set; }

	public string? ChildRestlessness { get; set; }

	public string? ChildAssessment { get; set; }

	public string? SkinPinchBack { get; set; }

	public string? CapillaryRefill { get; set; }

	public string? ChildDrinkingBreastfeeding { get; set; }

	public string? Muac { get; set; }

	// Adult assessment

	public string? AdultConsciousnessResponse { get; set; }

	public string? PatientClinicalState { get; set; }

	public string? IllnessFunctionalImpact { get; set; }


	// =========================================================
	// 07 — TREATMENT / MANAGEMENT
	// =========================================================

	public string? OutpatientTreatment { get; set; }

	public string? TreatmentGiven { get; set; }

	public string? Admitted { get; set; }

	public string? AdmissionDate { get; set; }

	public string? AdmissionTime { get; set; }

	public string? OralRehydration { get; set; }

	public string? IvRehydration { get; set; }

	public string? OtherTreatment { get; set; }


	// =========================================================
	// 08 — DISPOSITION / OUTCOME
	// =========================================================

	public string? Disposition { get; set; }

	public string? DispositionDate { get; set; }

	public string? DispositionTime { get; set; }

	public string? DischargeDeathDiagnosis { get; set; }


	// =========================================================
	// 09 — LABORATORY IDENTIFICATION
	// =========================================================

	public string? BacterialIdentification { get; set; }

	public string? ViralIdentification { get; set; }

	public string? ParasiteIdentification { get; set; }


	// =========================================================
	// 10 — WATER SOURCE / TREATMENT
	// =========================================================

	public List<string> WaterSourceSelections { get; set; } = new();

	public List<string> WaterTreatmentSelections { get; set; } = new();

	public string? OtherWaterSource { get; set; }

	public string? OtherWaterTreatment { get; set; }

	public string? WaterSourceType { get; set; }

	public string? WaterTreatment { get; set; }
}