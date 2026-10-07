import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

import '../models/truck.dart';

class TruckApi {
  TruckApi({
    Uri? baseUri,
    http.Client? client,
  }) : _baseUri = baseUri ??
            _configuredBaseUri(),
       _client = client ?? http.Client();

  final Uri _baseUri;
  final http.Client _client;

  Future<List<Truck>> getTrucks() async {
    final response = await _client.get(_endpoint('/api/trucks/'));
    _ensureSuccess(response);

    final decoded = jsonDecode(response.body) as List<dynamic>;
    return decoded
        .map((item) => Truck.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<void> createTruck({
    required String name,
    required String plate,
    required double lengthCm,
    required double widthCm,
    required double heightCm,
  }) async {
    final response = await _client.post(
      _endpoint('/api/trucks/'),
      headers: _jsonHeaders,
      body: jsonEncode({
        'name': name,
        'plate': plate,
        'lengthCm': lengthCm,
        'widthCm': widthCm,
        'heightCm': heightCm,
      }),
    );
    _ensureSuccess(response);
  }

  Future<void> createCargo({
    required int truckId,
    required String name,
    required double lengthCm,
    required double widthCm,
    required double heightCm,
    required int quantity,
  }) async {
    final response = await _client.post(
      _endpoint('/api/trucks/$truckId/cargo'),
      headers: _jsonHeaders,
      body: jsonEncode({
        'name': name,
        'lengthCm': lengthCm,
        'widthCm': widthCm,
        'heightCm': heightCm,
        'quantity': quantity,
      }),
    );
    _ensureSuccess(response);
  }

  Future<void> deleteTruck(int truckId) async {
    final response = await _client.delete(_endpoint('/api/trucks/$truckId'));
    _ensureSuccess(response);
  }

  Future<void> deleteCargo({
    required int truckId,
    required int cargoId,
  }) async {
    final response = await _client.delete(
      _endpoint('/api/trucks/$truckId/cargo/$cargoId'),
    );
    _ensureSuccess(response);
  }

  Uri _endpoint(String path) => _baseUri.resolve(path);

  static Uri _configuredBaseUri() {
    const configuredUrl = String.fromEnvironment('API_BASE_URL');
    if (configuredUrl.isNotEmpty) {
      return Uri.parse(configuredUrl);
    }

    return Uri.parse(
      kIsWeb ? 'http://localhost:8080' : 'http://10.0.2.2:8080',
    );
  }

  static const _jsonHeaders = {'Content-Type': 'application/json'};

  void _ensureSuccess(http.Response response) {
    if (response.statusCode >= 200 && response.statusCode < 300) {
      return;
    }

    var message = 'Falha na comunicação com a API (${response.statusCode}).';
    try {
      final body = jsonDecode(response.body);
      if (body is Map<String, dynamic> && body['message'] is String) {
        message = body['message'] as String;
      }
    } on FormatException {
      if (response.body.trim().isNotEmpty) {
        message = response.body;
      }
    }

    throw ApiException(message, response.statusCode);
  }
}

class ApiException implements Exception {
  const ApiException(this.message, this.statusCode);

  final String message;
  final int statusCode;

  @override
  String toString() => message;
}
