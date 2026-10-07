import 'package:flutter/material.dart';

import 'screens/truck_dashboard_screen.dart';
import 'services/truck_api.dart';

void main() {
  runApp(const CubagemCaminhaoApp());
}

class CubagemCaminhaoApp extends StatelessWidget {
  const CubagemCaminhaoApp({super.key, this.api});

  final TruckApi? api;

  @override
  Widget build(BuildContext context) {
    const brandColor = Color(0xFF145C52);

    return MaterialApp(
      title: 'CubagemCaminhao',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: brandColor,
          primary: brandColor,
        ),
        useMaterial3: true,
        scaffoldBackgroundColor: const Color(0xFFF5F7F6),
        inputDecorationTheme: const InputDecorationTheme(
          border: OutlineInputBorder(),
        ),
      ),
      home: TruckDashboardScreen(api: api ?? TruckApi()),
    );
  }
}
