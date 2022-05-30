
namespace BilliardGame.EventHandlers
{
    public struct GameInputEvent : IGameEvent
    {
        public enum States{
            Default,
            HorizontalAxisMovement,
            VerticalAxisMovement,
            Release,
            Paused
        }

        public float axisOffset;

        public States State;
    }
}
