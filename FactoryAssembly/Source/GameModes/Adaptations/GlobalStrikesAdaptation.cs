namespace FactoryAssembly
{
    internal class GlobalStrikesAdaptation : FactoryGameModeAdaptation
    {
        internal override void OnStartBomb(FactoryBomb previousBomb, FactoryBomb thisBomb)
        {
            if (previousBomb != null)
            {
                thisBomb.InternalBomb.NumStrikes = previousBomb.InternalBomb.NumStrikes;
                thisBomb.InternalBomb.StrikeIndicator.StrikeCount = previousBomb.InternalBomb.StrikeIndicator.StrikeCount;
                thisBomb.Timer.SetRateModifier(previousBomb.Timer.GetRate());
            }
        }
    }
}
