using System.Collections.Generic;

public interface IStateMachine
{
    public void ChangeState(IState state, List<string> data);
    public void ResetStateMachine();
}
