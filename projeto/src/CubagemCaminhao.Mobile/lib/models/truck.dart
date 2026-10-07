class Truck {
  const Truck({
    required this.id,
    required this.name,
    required this.plate,
    required this.lengthCm,
    required this.widthCm,
    required this.heightCm,
    required this.truckVolumeM3,
    required this.cargoVolumeM3,
    required this.occupancyPercent,
    required this.cargoItems,
  });

  final int id;
  final String name;
  final String plate;
  final double lengthCm;
  final double widthCm;
  final double heightCm;
  final double truckVolumeM3;
  final double cargoVolumeM3;
  final double occupancyPercent;
  final List<CargoItem> cargoItems;

  factory Truck.fromJson(Map<String, dynamic> json) {
    final cargoJson = json['cargoItems'] as List<dynamic>;

    return Truck(
      id: json['id'] as int,
      name: json['name'] as String,
      plate: json['plate'] as String,
      lengthCm: _readNumber(json['lengthCm']),
      widthCm: _readNumber(json['widthCm']),
      heightCm: _readNumber(json['heightCm']),
      truckVolumeM3: _readNumber(json['truckVolumeM3']),
      cargoVolumeM3: _readNumber(json['cargoVolumeM3']),
      occupancyPercent: _readNumber(json['occupancyPercent']),
      cargoItems: cargoJson
          .map((item) => CargoItem.fromJson(item as Map<String, dynamic>))
          .toList(),
    );
  }

  static double _readNumber(dynamic value) {
    if (value is num) {
      return value.toDouble();
    }
    if (value is String) {
      final parsed = double.tryParse(value);
      if (parsed != null) {
        return parsed;
      }
    }
    throw FormatException('Valor numérico inválido na resposta da API: $value');
  }
}

class CargoItem {
  const CargoItem({
    required this.id,
    required this.name,
    required this.lengthCm,
    required this.widthCm,
    required this.heightCm,
    required this.quantity,
    required this.totalVolumeM3,
  });

  final int id;
  final String name;
  final double lengthCm;
  final double widthCm;
  final double heightCm;
  final int quantity;
  final double totalVolumeM3;

  factory CargoItem.fromJson(Map<String, dynamic> json) {
    return CargoItem(
      id: json['id'] as int,
      name: json['name'] as String,
      lengthCm: Truck._readNumber(json['lengthCm']),
      widthCm: Truck._readNumber(json['widthCm']),
      heightCm: Truck._readNumber(json['heightCm']),
      quantity: json['quantity'] as int,
      totalVolumeM3: Truck._readNumber(json['totalVolumeM3']),
    );
  }
}
