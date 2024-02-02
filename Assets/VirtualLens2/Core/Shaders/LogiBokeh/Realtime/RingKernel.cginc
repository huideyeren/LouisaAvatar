#ifndef VIRTUALLENS2_LOGIBOKEH_RING_KERNEL_CGINC
#define VIRTUALLENS2_LOGIBOKEH_RING_KERNEL_CGINC

#include "../../Common/Constants.cginc"

static const int   NUM_RINGS       = 24;
static const int   RING_DENSITY    = 8;
static const float AREA_PER_SAMPLE = 4.0;

static const int RING_DENSITIES[24] = {
	1,
	8,
	16,
	24,
	32,
	40,
	48,
	56,
	64,
	72,
	80,
	88,
	96,
	104,
	112,
	120,
	128,
	136,
	144,
	152,
	160,
	168,
	176,
	184,
};

static const float RING_BORDER_RADIUSES[26] = {
	0.0,
	1.1283791670955126,
	3.385137501286538,
	5.641895835477563,
	7.898654169668588,
	10.155412503859614,
	12.412170838050638,
	14.668929172241665,
	16.925687506432688,
	19.182445840623714,
	21.43920417481474,
	23.695962509005764,
	25.95272084319679,
	28.209479177387816,
	30.46623751157884,
	32.722995845769866,
	34.97975417996089,
	37.23651251415192,
	39.493270848342945,
	41.750029182533964,
	44.00678751672499,
	46.26354585091602,
	48.520304185107044,
	50.77706251929806,
	53.03382085348909,
	55.290579187680116,
};

static const float RING_CENTER_RADIUSES[24] = {
	0.0,
	2.5231325220201604,
	4.652426491681278,
	6.863662517577698,
	9.09728368293446,
	11.340070282773889,
	13.587484461319491,
	15.837556323903858,
	18.089294154193436,
	20.3421447256411,
	22.595775212167435,
	24.849973424463737,
	27.104597715372737,
	29.359549924780953,
	31.614759885488656,
	33.870176111239545,
	36.12575996921784,
	38.381481905452574,
	40.63731892634535,
	42.89325287434272,
	45.14926922023208,
	47.40535620009159,
	49.661504187368415,
	51.91770522860519,
};

int ring_density(int ring_id){
	return RING_DENSITIES[ring_id];
}

float ring_border_radius(int ring_id){
	return RING_BORDER_RADIUSES[ring_id];
}

float ring_center_radius(int ring_id){
	return RING_CENTER_RADIUSES[ring_id];
}

float ring_angular_step(int ring_id){
	return 2.0 * PI / ring_density(ring_id);
}

float ring_angular_offset(int ring_id){
	return (ring_id & 1) * ring_angular_step(ring_id) * 0.5;
}

float2 ring_offset(int ring_id, int index){
	const float theta_step   = 2.0 * PI / ring_density(ring_id);
	const float theta_offset = (ring_id & 1) * (theta_step * 0.5);
	const float theta        = theta_step * index + theta_offset;
	return float2(cos(theta), sin(theta)) * ring_center_radius(ring_id);
}

#endif
