using UnityEngine;

public enum Occupation
{
    Student,
    SoftwareDeveloper,
    Chef,
    Lecturer,
    Retiree
}

public enum Talent
{
    Painting,
    Dancing,
    Singing,
    Acting
}

public enum Personality
{
    Energetic,
    Calm,
    Witty,
    Playful,
    Generous
}

public enum Hobby
{
    PlayingCarrom,
    WatchingDramaSeries,
    PlayingMobileLegends,
    ChattingAtKopitiam,
    Mahjong,
    MorningMarketShopping,
    SingingKaraoke,
    CollectingStamps,
    CyclingAroundTown,
    Photography
}

public enum FavouriteFood
{
    HainaneseChickenRice,
    CharKwayTeow,
    NasiLemak,
    CurryMee,
    WantanMee,
    DimSum,
    BakKutTeh,
    KayaToast,
    IceKacang
}

public enum FavouriteDrink
{
    TehTarik,
    KopiO,
    MiloIce,
    BarleyDrink,
    SirapBandung
}

public class NpcInfo : MonoBehaviour
{
    [SerializeField] private string npcName = "";
    [SerializeField] private Occupation npcOccupation;
    [SerializeField] private Talent npcTalent;
    [SerializeField] private Personality npcPersonality;
    [SerializeField] private Hobby npcHobby;
    [SerializeField] private FavouriteFood npcFavouriteFood;
    [SerializeField] private FavouriteDrink npcFavouriteDrink;

    public Sprite agentAvatar;

    public string GetPrompt()
    {
        return $"NPC Name: {npcName}\n" +
               $"NPC Occupation: {npcOccupation}\n" +
               $"NPC Talent: {npcTalent}\n" +
               $"NPC Personality: {npcPersonality}\n" +
               $"NPC Hobby: {npcHobby}\n" +
               $"NPC Favourite Food: {npcFavouriteFood}\n" +
               $"NPC Favourite Drink: {npcFavouriteDrink}\n";
    }

    // Getters
    public string GetNpcName() => npcName;
    public Talent GetTalent() => npcTalent;
    public FavouriteFood GetFavouriteFood() => npcFavouriteFood;
    public FavouriteDrink GetFavouriteDrink() => npcFavouriteDrink;
}
