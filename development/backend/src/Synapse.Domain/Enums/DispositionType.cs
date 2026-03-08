namespace Synapse.Domain.Enums;

/// <summary>不良品処置区分。</summary>
public enum DispositionType
{
    Rework                 = 1,
    Scrap                  = 2,
    ConditionalAcceptance  = 3,
    Return                 = 4,
    Hold                   = 5,
}
