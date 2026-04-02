using System;

public interface IHpNotifier
{
    event Action<float> ActionOnCurHpChanged;
    event Action<float> ActionOnMaxHpChanged;
}