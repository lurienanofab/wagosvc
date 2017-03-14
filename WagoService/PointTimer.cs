using System.Threading;
using WagoService.Actions;

namespace WagoService
{
    public class PointTimer
    {
        protected Timer _timer;
        protected bool _cancel;

        public IControlAction Action { get; private set; }
        //public Point Point { get; private set; }
        public IRequestQueue Queue { get; private set; }
        public uint Duration { get; private set; }
        //public bool State { get; private set; }

        public PointTimer(IRequestQueue queue, IControlAction action, uint duration)
        {
            Action = action;
            Queue = queue;
            Duration = duration;
            _timer = new Timer(x => ExecuteFutureAction(), null, duration * 1000, Timeout.Infinite);
        }

        public void Cancel(bool execute)
        {
            Log.Write("CANCEL:{0}:Duration={1}:Execute={2}", Action.GetLogMessage(), Duration, execute);

            if (execute)
                ExecuteFutureAction();

            _cancel = true;
        }

        protected virtual void ExecuteFutureAction()
        {
            Log.Write("EXEC:{0}:Duration={1}{2}", Action.GetLogMessage(), Duration, (_cancel) ? " (cancelled)" : string.Empty);

            if (!_cancel)
            {
                Queue.Push(Action);
                Remove(); //only when this PointTimer is not cancelled because a new one might exist now
            }
        }

        protected void Remove()
        {
            PointTimer pt;
            FutureActionManager.Current.TryRemove(Action.ActionID, out pt);
        }
    }
}
