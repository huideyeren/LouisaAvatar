float4 LCDMain(float2 texcoord){
    int x = _Type;
    float2 uv0 = (texcoord * float2(0.5, 1)) + float2(x * 0.5, 0) ;
    float2 uv = texcoord;
    float4 c = LIL_SAMPLE_2D(_MainTex, sampler_MainTex, uv0);
    if(_Type == 0){
        float lcdw = ClockLCDW(uv);
        c.rgb = saturate(c.rgb - lcdw) + lcdw * pow(float3(205, 210, 202) / 255.0f,2.2);
    } else if(_Type == 1){
        float lcdb = ClockLCDB(uv);
        c.rgb = saturate(c.rgb - lcdb) + lcdb * pow(float3(35, 24, 21) / 255.0f,2.2);
    }
    return c;
}