using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;

namespace FeastsOfCalradia.Behaviors
{
    public class ClanHeroesProbeBehavior : CampaignBehaviorBase
    {
        private int _listingCount;

        public int ListingCount
        {
            get { return _listingCount; }
        }

        public override void RegisterEvents()
        {
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("FeastsOfCalradia_ClanHeroesProbe_ListingCount", ref _listingCount);
        }

        public string GetClanHeroSummary()
        {
            _listingCount++;

            // VERIFY: Hero.MainHero?.Clan is the right traversal. Clan.PlayerClan is the alternative
            // and should always be non-null in a campaign; using it here for that reason.
            Clan clan = Clan.PlayerClan;
            if (clan == null)
            {
                return "No player clan.";
            }

            // VERIFY: Clan.Heroes returns all heroes in the clan, alive and dead, including main hero
            // and companions. Confirm against decompiled Clan class if anything looks off in-game.
            var heroes = clan.Heroes;
            var sb = new StringBuilder();
            sb.AppendLine("Clan " + clan.Name + ": " + heroes.Count + " heroes (listing #" + _listingCount + ")");
            foreach (Hero hero in heroes)
            {
                sb.AppendLine("  - " + hero.Name + " (" + (hero.IsAlive ? "alive" : "dead") + ")");
            }
            return sb.ToString();
        }
    }
}
