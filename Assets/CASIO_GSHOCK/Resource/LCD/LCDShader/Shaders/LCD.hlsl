int _Month, _Day, _DayOfWeek, _Hour, _Minute, _Second;
int _Type;

float mod(float a, float b)
{
    return a - floor(a / b) * b;
}

float LCDSeg(float2 uv, int num, int type)
{
    float c = 0;
    float d = 7 - type;
    float2 st = (uv + float2(num, d)) / float2(16.0, 8.0);
    bool isT = st.x > num / 16.0 && st.x <= (num + 1) / 16.0 && st.y > d / 8 && st.y <= (d + 1) / 8.0;
    c = isT ? LIL_SAMPLE_2D(_LCD, sampler_LCD, st).a : 0;
    return c;
}

float LCDSegDoD(float2 uv, uint num, int type)
{
    uint x = num % 2;
    uint y = 7 - num / 2 - type * 4;
    float2 st = (uv * float2(0.5, 1) + float2(x + 6, y)) / float2(8.0, 8.0);
    bool isT = st.x > (x + 6) / 8.0 && st.x < (x + 7) / 8.0 && st.y > y / 8.0 && st.y < (y + 1) / 8.0;
    float c = isT ? LIL_SAMPLE_2D(_LCD, sampler_LCD, st).a : 0;
    return c;
}

float LCDSegPM(float2 uv, int type)
{
    float2 st = (uv + float2(15, (1 - type) * 4)) / float2(16.0, 8.0);
    bool isT = st.x > 15 / 16.0 && st.y > (1 - type) * 0.5 && st.y < ((1 - type) * 4 + 1) * 0.125;
    return isT ? LIL_SAMPLE_2D(_LCD, sampler_LCD, st).a : 0;
}

float MonthDisB(float2 uv)
{
    float2 uv0 = uv * 8.25;
    uv0 += float2(-3.675, -4.5);
    float2 uv1 = uv * 8.25;
    uv1 += float2(-4.255, -4.5);
    int t = floor(_Month * 0.1);
    int o = _Month - t * 10;
    if (t == 0)
    {
        return LCDSeg(uv1, o, 3);
    }
    else
    {
        return LCDSeg(uv0, t, 3) + LCDSeg(uv1, o, 3);
    }
}

float DayDisB(float2 uv)
{
    float2 uv0 = uv * 8.25;
    uv0 += float2(-5.21, -4.5);
    float2 uv1 = uv * 8.25;
    uv1 += float2(-5.805, -4.5);
    int t = floor(_Day * 0.1);
    int o = _Day - t * 10;
    if (t == 0)
    {
        return LCDSeg(uv1, o, 3);
    }
    else
    {
        return LCDSeg(uv0, t, 3) + LCDSeg(uv1, o, 3);
    }
}

float DayOfWeekDisB(float2 uv)
{
    uv *= 8.2;
    uv += float2(-1.85, -4.475);
    return LCDSegDoD(uv, mod(_DayOfWeek - 1, 7), 0);
}

float HourDisB(float2 uv)
{
    float2 uv0 = uv * 6.25;
    uv0 += float2(-0.925, -2.025);
    float2 uv1 = uv * 6.25;
    uv1 += float2(-1.6, -2.025);
    float h = _Hour % 12;
    if (h == 0)
        h = 12;
    int t = floor(h * 0.1);
    int o = h - t * 10;
    if (t == 0)
    {
        return LCDSeg(uv1, o, 0);
    }
    else
    {
        return LCDSeg(uv0, t, 1) + LCDSeg(uv1, o, 0);
    }
}

float AmpmDisB(float2 uv, int type)
{
    float2 uv0 = uv * 12.25;
    uv0 += float2(-2.31, -5.79);
    if (_Hour < 12)
    {
        return 0;
    }
    else
    {
        return LCDSegPM(uv0, type);
    }
}

float MinuteDisB(float2 uv)
{
    float2 uv0 = uv * 6.25;
    uv0 += float2(-2.525, -2.025);
    float2 uv1 = uv * 6.25;
    uv1 += float2(-3.225, -2.025);
    float t = floor(_Minute * 0.1);
    float o = _Minute - t * 10;
    return LCDSeg(uv0, t, 0) + LCDSeg(uv1, o, 0);
}

float SecondDisB(float2 uv)
{
    float2 uv0 = uv * 8.25;
    uv0 += float2(-5.225, -2.65);
    float2 uv1 = uv * 8.25;
    uv1 += float2(-5.925, -2.65);
    float t = floor(_Second * 0.1);
    float o = _Second - t * 10;
    return LCDSeg(uv0, t, 2) + LCDSeg(uv1, o, 2);
}

float MonthDisW(float2 uv)
{
    float2 uv0 = uv * 8;
    uv0 += float2(-3.625, -4.351);
    float2 uv1 = uv * 8;
    uv1 += float2(-4.255, -4.351);
    int t = floor(_Month * 0.1);
    int o = _Month - t * 10;
    if (t == 0)
    {
        return LCDSeg(uv1, o, 7);
    }
    else
    {
        return LCDSeg(uv0, t, 7) + LCDSeg(uv1, o, 7);
    }
}

float DayDisW(float2 uv)
{
    float2 uv0 = uv * 8;
    uv0 += float2(-5.085, -4.351);
    float2 uv1 = uv * 8;
    uv1 += float2(-5.715, -4.351);
    int t = floor(_Day * 0.1);
    int o = _Day - t * 10;
    if (t == 0)
    {
        return LCDSeg(uv1, o, 7);
    }
    else
    {
        return LCDSeg(uv0, t, 7) + LCDSeg(uv1, o, 7);
    }
}

float DayOfWeekDisW(float2 uv)
{
    uv *= 8;
    uv += float2(-1.99, -4.358);
    return LCDSegDoD(uv, mod(_DayOfWeek - 1, 7), 1);
}

float HourDisW(float2 uv)
{
    float2 uv0 = uv * 6.;
    uv0 += float2(-0.866, -1.91);
    float2 uv1 = uv * 6;
    uv1 += float2(-1.51, -1.91);
    float h = _Hour % 12;
    if (h == 0)
        h = 12;
    int t = floor(h * 0.1);
    int o = h - t * 10;
    if (t == 0)
    {
        return LCDSeg(uv1, o, 4);
    }
    else
    {
        return LCDSeg(uv0, t, 5) + LCDSeg(uv1, o, 4);
    }
}

float AmpmDisW(float2 uv, int type)
{
    float2 uv0 = uv * 12.;
    uv0 += float2(-2.3, -5.72);
    if (_Hour < 12)
    {
        return 0;
    }
    else
    {
        return LCDSegPM(uv0, type);
    }
}

float MinuteDisW(float2 uv)
{
    float2 uv0 = uv * 6;
    uv0 += float2(-2.39, -1.91);
    float2 uv1 = uv * 6;
    uv1 += float2(-3.08, -1.91);
    float t = floor(_Minute * 0.1);
    float o = _Minute - t * 10;
    return LCDSeg(uv0, t, 4) + LCDSeg(uv1, o, 4);
}

float SecondDisW(float2 uv)
{
    float2 uv0 = uv * 8;
    uv0 += float2(-5.09, -2.54);
    float2 uv1 = uv * 8;
    uv1 += float2(-5.71, -2.54);
    float t = floor(_Second * 0.1);
    float o = _Second - t * 10;
    return LCDSeg(uv0, t, 6) + LCDSeg(uv1, o, 6);
}

float ClockLCDB(float2 uv)
{
    return MonthDisB(uv) + DayDisB(uv) + DayOfWeekDisB(uv) + HourDisB(uv) + MinuteDisB(uv) + SecondDisB(uv) + AmpmDisB(uv, 0);
}

float ClockLCDW(float2 uv)
{
    return MonthDisW(uv) + DayDisW(uv) + DayOfWeekDisW(uv) + HourDisW(uv) + MinuteDisW(uv) + SecondDisW(uv) + AmpmDisW(uv, 1);
}

