using System.Threading;
using WagoService.Actions;

namespace WagoService
{
    public class PointTimer
    {
        protected Timer _timer;
        protected bool _cancel;

        public PointAction Action { get; private set; }
        //public Point Point { get; private set; }
        public IRequestQueue Queue { get; private set; }
        public uint Duration { get; private set; }
        //public bool State { get; private set; }

        public PointTimer(IRequestQueue queue, PointAction action, uint duration)
        {
            Action = action;
            Queue = queue;
            Duration = duration;
            _timer = new Timer(x => ExecuteFutureAction(), null, duration * 1000, Timeout.Infinite);
        }

        public void Cancel(bool execute)
        {
            Log.Write($"CANCEL:PointID={Action.PointID}:Duration={Duration}:Execute={execute}");

            if (execute)
                ExecuteFutureAction();

            _cancel = true;
        }

        protected virtual void ExecuteFutureAction()
        {
            var cancelled = _cancel ? " (cancelled)" : string.Empty;
            Log.Write($"EXEC:PointID={Action.PointID}:Duration={Duration}{cancelled}");

            if (!_cancel)
            {
                Queue.Push(Action);
                Remove(); //only when this PointTimer is not cancelled because a new one might exist now
            }
        }

        protected void Remove()
        {
            FutureActionManager.Current.TryRemove(Action.PointID, out PointTimer pt);
        }
    }
}
