namespace Forge.Core.Airship.Data{
    public class AirshipBuff{
        #region BuffType enum

        /// <summary>
        ///   Defines different kinds of statistics that can be affected by a buff.
        /// </summary>
        public enum BuffType{
            MaxVelocity,
            MaxTurnRate,
            MaxAscentRate,
            MaxAcceleration,
            MaxTurnAcceleration,
            MaxAscentAcceleration
        }

        #endregion

        public readonly int Id;
        public readonly float Modifier;
        public readonly BuffType Type;

        public AirshipBuff(BuffType buffType, float modifier){
            Type = buffType;
            Modifier = modifier;
            Id = (int) buffType*10000 + (int) (modifier*1000);
        }
    }
}