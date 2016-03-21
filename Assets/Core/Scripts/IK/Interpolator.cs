using UnityEngine;
using System;
using System.Collections;

public enum InterpolationState
{
    Min,
    Max,
    ToMin,
    ToMax,
}

public class Interpolator<T>
{
    private InterpolationState state;
    public InterpolationState State
    {
        get { return this.state; }
    }

    private T maxValue;
    private T minValue;

    private float startTime;
    private float stopTime;

    private Func<T, T, float, T> lerp;

    public T Value { get { return this.GetValue(); } }

    public Interpolator(
        T min, 
        T max, 
        Func<T, T, float, T> lerp)
    {
        this.maxValue = max;
        this.minValue = min;
        this.state = InterpolationState.Min;
        this.lerp = lerp;
    }

    public void ToMax(float delay)
    {
        float time = Time.time;

        if (this.state == InterpolationState.Min)
        {
            this.startTime = time;
            this.stopTime = time + delay;
            this.state = InterpolationState.ToMax;
        }
        else if (this.state == InterpolationState.ToMin)
        {
            this.FlipTime(time, delay);
            this.state = InterpolationState.ToMax;
        }
    }

    public void ToMin(float delay)
    {
        float time = Time.time;

        if (this.state == InterpolationState.Max)
        {
            this.startTime = time;
            this.stopTime = time + delay;
            this.state = InterpolationState.ToMin;
        }
        else if (this.state == InterpolationState.ToMax)
        {
            this.FlipTime(time, delay);
            this.state = InterpolationState.ToMin;
        }
    }

    private void FlipTime(float time, float delay)
    {
        float timeElapsed = time - this.startTime;
        float timeRemaining = this.stopTime - time;

        float totalTime = timeElapsed + timeRemaining;
        float scale = delay / totalTime;

        this.startTime = time - (timeRemaining * scale);
        this.stopTime = time + (timeElapsed * scale);
    }

    public void ForceMax()
    {
        this.state = InterpolationState.Max;
    }

    public void ForceMin()
    {
        this.state = InterpolationState.Min;
    }

    protected T GetValue()
    {
        float time = Time.time;
        float t = 0.0f;

        if (this.state == InterpolationState.ToMax)
        {
            if (time > this.stopTime)
                this.state = InterpolationState.Max;
            else
                t = Time.time - this.startTime;
        }
        else if (this.state == InterpolationState.ToMin)
        {
            if (time > this.stopTime)
                this.state = InterpolationState.Min;
            else
                t = this.stopTime - Time.time;
        }

        if (this.state == InterpolationState.Min)
            return this.minValue;
        else if (this.state == InterpolationState.Max)
            return this.maxValue;

        t /= (this.stopTime - this.startTime);
        return
            this.lerp(this.minValue, this.maxValue, t);
    }

    public void SetValues(T min, T max)
    {
        this.minValue = min;
        this.maxValue = max;
    }
}