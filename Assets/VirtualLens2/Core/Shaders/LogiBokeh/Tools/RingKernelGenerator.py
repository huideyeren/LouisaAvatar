import math
import argparse


def main():
    parser = argparse.ArgumentParser(description='Ring Kernel Generator')
    parser.add_argument('-n', '--num_rings', type=int, help='Maximum number of rings')
    parser.add_argument('-a', '--area_per_sample', type=float, default=4.0, help='Area per sample [px]')
    args = parser.parse_args()

    num_rings       = args.num_rings
    density         = 8
    area_per_sample = args.area_per_sample

    def ring_density(r):
        if r == 0:
            return 1;
        return density * r

    def ring_border_radius(r):
        if r == 0:
            return 0.0
        k = 1 + (r * (r - 1) / 2) * density;
        return math.sqrt(area_per_sample * k / math.pi)

    def ring_center_radius(r):
        if r == 0:
            return 0.0
        r0 = ring_border_radius(r)
        r1 = ring_border_radius(r + 1)
        return math.sqrt(0.5 * (r0 * r0 + r1 * r1))

    print('#ifndef VIRTUALLENS2_LOGIBOKEH_RING_KERNEL_CGINC')
    print('#define VIRTUALLENS2_LOGIBOKEH_RING_KERNEL_CGINC')
    print('')

    print('#include "../../Common/Constants.cginc"')
    print('')

    print(f'static const int   NUM_RINGS       = {num_rings};')
    print(f'static const int   RING_DENSITY    = {density};')
    print(f'static const float AREA_PER_SAMPLE = {area_per_sample};')
    print('')

    print(f'static const int RING_DENSITIES[{num_rings}] = {{')
    for i in range(num_rings):
        print(f'\t{ring_density(i)},')
    print(f'}};')
    print('')

    print(f'static const float RING_BORDER_RADIUSES[{num_rings+2}] = {{')
    for i in range(num_rings + 2):
        print(f'\t{ring_border_radius(i)},')
    print(f'}};')
    print('')

    print(f'static const float RING_CENTER_RADIUSES[{num_rings}] = {{')
    for i in range(num_rings):
        print(f'\t{ring_center_radius(i)},')
    print(f'}};')
    print('')

    print('int ring_density(int ring_id){')
    print('\treturn RING_DENSITIES[ring_id];')
    print('}')
    print('')

    print('float ring_border_radius(int ring_id){')
    print('\treturn RING_BORDER_RADIUSES[ring_id];')
    print('}')
    print('')

    print('float ring_center_radius(int ring_id){')
    print('\treturn RING_CENTER_RADIUSES[ring_id];')
    print('}')
    print('')

    print('float ring_angular_step(int ring_id){')
    print('\treturn 2.0 * PI / ring_density(ring_id);')
    print('}')
    print('')

    print('float ring_angular_offset(int ring_id){')
    print('\treturn (ring_id & 1) * ring_angular_step(ring_id) * 0.5;')
    print('}')
    print('')

    print('float2 ring_offset(int ring_id, int index){')
    print('\tconst float theta_step   = 2.0 * PI / ring_density(ring_id);')
    print('\tconst float theta_offset = (ring_id & 1) * (theta_step * 0.5);')
    print('\tconst float theta        = theta_step * index + theta_offset;')
    print('\treturn float2(cos(theta), sin(theta)) * ring_center_radius(ring_id);')
    print('}')
    print('')

    print('#endif')


if __name__ == '__main__':
    main()
