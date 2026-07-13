using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 主角（海豹）类
/// </summary>
public class Seal : MonoBehaviour
{
    #region 氧气系统

    private float OxygenValue;
    public float OxygenValueProperty
    {
        get { return OxygenValue; }
        private set
        {
            OxygenValue = value;
        }
    }
    [SerializeField]public float OxygenMaxValue = 100f;

    #endregion

    #region 状态机

    public LifeStateMachine LifeStateMachine { get; private set; }
    public OxygenStateMachine OxygenStateMachine { get; private set; }
    public EnvironmentStateMachine EnvironmentStateMachine { get; private set; }
    public ActionStateMachine ActionStateMachine { get; private set; }

    #endregion

    #region Unity 生命周期

    private void Awake()
    {
        LifeStateMachine = new LifeStateMachine(this);
        OxygenStateMachine = new OxygenStateMachine(this);
        EnvironmentStateMachine = new EnvironmentStateMachine(this);
        ActionStateMachine = new ActionStateMachine(this);
    }

    private void Update()
    {
        LifeStateMachine.Update();
        OxygenStateMachine.Update();
        EnvironmentStateMachine.Update();
        ActionStateMachine.Update();
    }

    private void FixedUpdate()
    {
        LifeStateMachine.FixedUpdate();
        OxygenStateMachine.FixedUpdate();
        EnvironmentStateMachine.FixedUpdate();
        ActionStateMachine.FixedUpdate();
    }

    #endregion
}

