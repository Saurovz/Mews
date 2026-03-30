using System.Runtime.Serialization;

namespace TaxManager.Domain.Enums;

public enum Currency
{
    [EnumMember(Value = "UnitedArabEmiratesDirham")] 
    AED = 1,
    [EnumMember(Value = "AustralianDollar")] 
    AUD,
    [EnumMember(Value = "NetherlandsAntilleanGuilder")] 
    ANG,
    [EnumMember(Value = "ArgentinianPeso")] 
    ARS,
    [EnumMember(Value = "BolivianBoliviano")] 
    BOB,
    [EnumMember(Value = "BrazilianReal")] 
    BRL,
    [EnumMember(Value = "BahamianDollar")] 
    BSD,
    [EnumMember(Value = "CanadianDollar")] 
    CAD,
    [EnumMember(Value = "SwissFranc")] 
    CHF,
    [EnumMember(Value = "ChileanPeso")] 
    CLP,
    [EnumMember(Value = "ChineseYuan")] 
    CNY,
    [EnumMember(Value = "ColombianPeso")] 
    COP,
    [EnumMember(Value = "CongoleseFranc")] 
    CDF,
    [EnumMember(Value = "CostaRicanColon")] 
    CRC,
    [EnumMember(Value = "CzechKoruna")] 
    CZK,
    [EnumMember(Value = "DanishKrone")] 
    DKK,
    [EnumMember(Value = "DominicanPeso")] 
    DOP,
    [EnumMember(Value = "EgyptianPound")] 
    EGP,
    [EnumMember(Value = "Euro")] 
    EUR,
    [EnumMember(Value = "FijianDollar")] 
    FJD,
    [EnumMember(Value = "BritishPound")] 
    GBP,
    [EnumMember(Value = "GeorgianLari")] 
    GEL,
    [EnumMember(Value = "GhanaianCedi")] 
    GHS,
    [EnumMember(Value = "GuatemalanQuetzal")] 
    GTQ,
    [EnumMember(Value = "HongKongDollar")] 
    HKD,
    [EnumMember(Value = "HondurianLempira")] 
    HNL,
    [EnumMember(Value = "CroatianKuna")] 
    HRK,
    [EnumMember(Value = "HungarianForint")] 
    HUF,
    [EnumMember(Value = "IndonesianRupiah")] 
    IDR,
    [EnumMember(Value = "IsraeliShekel")] 
    ILS,
    [EnumMember(Value = "IcelandicKrona")] 
    ISK,
    [EnumMember(Value = "JapaneseYen")] 
    JPY,
    [EnumMember(Value = "KenyanShilling")] 
    KES,
    [EnumMember(Value = "CambodianRiel")] 
    KHR,
    [EnumMember(Value = "KoreanWon")] 
    KRW,
    [EnumMember(Value = "LebanesePound")] 
    LBP,
    [EnumMember(Value = "MalagasyAriary")] 
    MGA,
    [EnumMember(Value = "MauritianRupee")] 
    MUR,
    [EnumMember(Value = "MexicanPeso")] 
    MXN,
    [EnumMember(Value = "MoroccanDirham")] 
    MAD,
    [EnumMember(Value = "MalaysianRinggit")] 
    MYR,
    [EnumMember(Value = "NamibianDollar")] 
    NAD,
    [EnumMember(Value = "NigerianNaira")] 
    NGN,
    [EnumMember(Value = "NordishKrona")] 
    NOK,
    [EnumMember(Value = "NewZealandDollar")] 
    NZD,
    [EnumMember(Value = "PanamanianBalboa")] 
    PAB,
    [EnumMember(Value = "PeruvianNuevoSol")] 
    PEN,
    [EnumMember(Value = "PhilippinePeso")] 
    PHP,
    [EnumMember(Value = "PolishZloty")] 
    PLN,
    [EnumMember(Value = "ParaguayanGuarani")] 
    PYG,
    [EnumMember(Value = "RussianRuble")] 
    RUB,
    [EnumMember(Value = "SaudiRiyal")] 
    SAR,
    [EnumMember(Value = "SeychellesRupee")] 
    SCR,
    [EnumMember(Value = "SwedishKrona")] 
    SEK,
    [EnumMember(Value = "SingaporeDollar")] 
    SGD,
    [EnumMember(Value = "ThaiBaht")] 
    THB,
    [EnumMember(Value = "TonganPaanga")] 
    TOP,
    [EnumMember(Value = "TurkishLira")] 
    TRY,
    [EnumMember(Value = "NewTaiwanDollar")] 
    TWD,
    [EnumMember(Value = "UkrainianHryvnia")] 
    UAH,
    [EnumMember(Value = "UnitedStatesDollar")] 
    USD,
    [EnumMember(Value = "UruguayanPeso")] 
    UYU,
    [EnumMember(Value = "VanuatuVatu")] 
    VUV,
    [EnumMember(Value = "SamoanTala")] 
    WST,
    [EnumMember(Value = "EastCaribbeanDollar")] 
    XCD,
    [EnumMember(Value = "WestAfricanFranc")] 
    XOF,
    [EnumMember(Value = "PacificFranc")] 
    XPF,
    [EnumMember(Value = "SouthAfricanRand")] 
    ZAR
}
