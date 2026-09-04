using System;
using System.Collections.Generic;

namespace EPS_Web_App.Data.Models;

public partial class AstRecord
{
    public long AstRecordId { get; set; }

    public string? MonthCollected { get; set; }

    public string? SpecimenId { get; set; }

    public string? Lan { get; set; }

    public string? SiteCode { get; set; }

    public string? ParticipantType { get; set; }

    public string? GtdStatus { get; set; }

    public string? DiarrheaMedicationPast72h { get; set; }

    public string? MedicationsPast72h { get; set; }

    public string? IllnessFunctionalImpact { get; set; }

    public string? BacterialIdentification { get; set; }

    public string? ViralIdentification { get; set; }

    public string? ParasiteIdentification { get; set; }

    public string? DiagnosticTest { get; set; }

    public string? MdrStatus { get; set; }

    public string? EsblStatus { get; set; }

    public string? AmikacinMic { get; set; }

    public string? AmikacinInt { get; set; }

    public string? AmoxicillinClavulanateMic { get; set; }

    public string? AmoxicillinClavulanateInt { get; set; }

    public string? AmpicillinSulbactamMic { get; set; }

    public string? AmpicillinSulbactamInt { get; set; }

    public string? AmpicillinMic { get; set; }

    public string? AmpicillinInt { get; set; }

    public string? AztreonamMic { get; set; }

    public string? AztreonamInt { get; set; }

    public string? CefazolinMic { get; set; }

    public string? CefazolinInt { get; set; }

    public string? CefepimeMic { get; set; }

    public string? CefepimeInt { get; set; }

    public string? CefotaximeMic { get; set; }

    public string? CefotaximeInt { get; set; }

    public string? CefotaximeClavulanateMic { get; set; }

    public string? CefotaximeClavulanateInt { get; set; }

    public string? CefotetanMic { get; set; }

    public string? CefotetanInt { get; set; }

    public string? CefoxitinMic { get; set; }

    public string? CefoxitinInt { get; set; }

    public string? CeftazidimeMic { get; set; }

    public string? CeftazidimeInt { get; set; }

    public string? CeftazidimeClavulanateMic { get; set; }

    public string? CeftazidimeClavulanateInt { get; set; }

    public string? CeftolozaneTazobactamMic { get; set; }

    public string? CeftolozaneTazobactamInt { get; set; }

    public string? CefuroximeMic { get; set; }

    public string? CefuroximeInt { get; set; }

    public string? CephalothinMic { get; set; }

    public string? CephalothinInt { get; set; }

    public string? CeftriaxoneMic { get; set; }

    public string? CeftriaxoneInt { get; set; }

    public string? ChloramphenicolMic { get; set; }

    public string? ChloramphenicolInt { get; set; }

    public string? CiprofloxacinMic { get; set; }

    public string? CiprofloxacinInt { get; set; }

    public string? ColistinMic { get; set; }

    public string? ColistinInt { get; set; }

    public string? ErtapenemMic { get; set; }

    public string? ErtapenemInt { get; set; }

    public string? FosfomycinMic { get; set; }

    public string? FosfomycinInt { get; set; }

    public string? GentamicinMic { get; set; }

    public string? GentamicinInt { get; set; }

    public string? GatifloxacinMic { get; set; }

    public string? GatifloxacinInt { get; set; }

    public string? ImipenemMic { get; set; }

    public string? ImipenemInt { get; set; }

    public string? LevofloxacinMic { get; set; }

    public string? LevofloxacinInt { get; set; }

    public string? MeropenemMic { get; set; }

    public string? MeropenemInt { get; set; }

    public string? MoxifloxacinMic { get; set; }

    public string? MoxifloxacinInt { get; set; }

    public string? NalidixicAcidMic { get; set; }

    public string? NalidixicAcidInt { get; set; }

    public string? NitrofurantoinMic { get; set; }

    public string? NitrofurantoinInt { get; set; }

    public string? NorfloxacinMic { get; set; }

    public string? NorfloxacinInt { get; set; }

    public string? PiperacillinTazobactamMic { get; set; }

    public string? PiperacillinTazobactamInt { get; set; }

    public string? PiperacillinMic { get; set; }

    public string? PiperacillinInt { get; set; }

    public string? TetracyclineMic { get; set; }

    public string? TetracyclineInt { get; set; }

    public string? TigecyclineMic { get; set; }

    public string? TigecyclineInt { get; set; }

    public string? TicarcillinClavulanateMic { get; set; }

    public string? TicarcillinClavulanateInt { get; set; }

    public string? TobramycinMic { get; set; }

    public string? TobramycinInt { get; set; }

    public string? TrimethoprimSulfamethoxazoleMic { get; set; }

    public string? TrimethoprimSulfamethoxazoleInt { get; set; }

    public string? AzithromycinMic { get; set; }

    public string? AzithromycinInt { get; set; }

    public string? RifampicinMic { get; set; }

    public string? RifampicinInt { get; set; }

    public string LinkageStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
