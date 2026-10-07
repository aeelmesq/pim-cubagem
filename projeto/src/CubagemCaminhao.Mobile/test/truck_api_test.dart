import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

import 'package:cubagemcaminhao/services/truck_api.dart';

void main() {
  test('loads trucks and cargo items from the REST response', () async {
    final api = TruckApi(
      baseUri: Uri.parse('http://example.test'),
      client: MockClient((request) async {
        expect(request.method, 'GET');
        expect(request.url.path, '/api/trucks/');
        return http.Response(
          jsonEncode([
            {
              'id': 7,
              'name': 'Caminhão baú',
              'plate': 'ABC1D23',
              'lengthCm': 500,
              'widthCm': 220,
              'heightCm': 240,
              'truckVolumeM3': 26.4,
              'cargoVolumeM3': 2.0,
              'occupancyPercent': 7.6,
              'cargoItems': [
                {
                  'id': 11,
                  'name': 'Caixas',
                  'lengthCm': 100,
                  'widthCm': 100,
                  'heightCm': 100,
                  'quantity': 2,
                  'totalVolumeM3': 2.0,
                },
              ],
            },
          ]),
          200,
          headers: {'content-type': 'application/json'},
        );
      }),
    );

    final trucks = await api.getTrucks();

    expect(trucks, hasLength(1));
    expect(trucks.single.plate, 'ABC1D23');
    expect(trucks.single.occupancyPercent, 7.6);
    expect(trucks.single.cargoItems.single.name, 'Caixas');
    expect(trucks.single.cargoItems.single.totalVolumeM3, 2.0);
  });

  test('surfaces validation messages returned by the API', () async {
    final api = TruckApi(
      baseUri: Uri.parse('http://example.test'),
      client: MockClient(
        (_) async => http.Response(
          jsonEncode({
            'message': 'Já existe um caminhão cadastrado com essa placa.',
          }),
          409,
          headers: {'content-type': 'application/json'},
        ),
      ),
    );

    expect(
      () => api.createTruck(
        name: 'Caminhão',
        plate: 'ABC1D23',
        lengthCm: 500,
        widthCm: 220,
        heightCm: 240,
      ),
      throwsA(
        isA<ApiException>().having(
          (error) => error.message,
          'message',
          'Já existe um caminhão cadastrado com essa placa.',
        ),
      ),
    );
  });
}
