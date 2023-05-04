using System.Collections.Generic;

public class FSM<E>
{
    Dictionary<E, GameState<E>> states;
    E _state;

    public FSM(E initState)
    {
        states = new Dictionary<E, GameState<E>>();
        _state = initState;
    }

    public E State
    {
        get
        {
            return _state;
        }
        set
        {
            GameState<E> cur = states[_state];
            cur.OnStateExit();
            _state = value;
            GameState<E> next = states[value];
            next.OnStateEnter();
        }
    }

    public void RegisterState(E e, GameState<E> state)
    {
        state.Init(this);
        states.Add(e, state);
    }

    public void OnStateLogic()
    {
        GameState<E> cur = states[_state];
        cur.OnStateLogic();
    }
}

public abstract class GameState<E>
{
    protected FSM<E> fsm;

    public void Init(FSM<E> fsm)
    {
        this.fsm = fsm;
    }

    public abstract void OnStateEnter();
    public abstract void OnStateLogic();
    public abstract void OnStateExit();
}
