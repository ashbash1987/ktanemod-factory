using System.Reflection;

namespace FactoryAssembly
{
    internal class GlobalTimerAdaptation : FactoryGameModeAdaptation
    {
        internal override void OnStartBomb(FactoryBomb previousBomb, FactoryBomb thisBomb)
        {
            if (previousBomb != null)
            {
                //Need to preserve the initial time value, otherwise 'remaining time' on the results screen is incorrectly adjusted
                FieldInfo initialTimeField = typeof(TimerComponent).GetField("initialTime", BindingFlags.NonPublic | BindingFlags.Instance);
                float initialTime = (float)initialTimeField.GetValue(previousBomb.Timer);

                thisBomb.Timer.SetTimeRemaing(previousBomb.Timer.TimeRemaining);

                //Set the initial time field back
                initialTimeField.SetValue(thisBomb.Timer, initialTime);
            }
        }
    }
}
