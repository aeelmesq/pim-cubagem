// This is a basic Flutter widget test.
//
// To perform an interaction with a widget in your test, use the WidgetTester
// utility in the flutter_test package. For example, you can send tap and scroll
// gestures. You can also use WidgetTester to find child widgets in the widget
// tree, read text, and verify that the values of widget properties are correct.

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

import 'package:cubagemcaminhao/main.dart';
import 'package:cubagemcaminhao/services/truck_api.dart';

void main() {
  testWidgets('Displays the CubagemCaminhao app shell', (WidgetTester tester) async {
    final api = TruckApi(
      baseUri: Uri.parse('http://test'),
      client: MockClient((_) async => httpResponse('[]')),
    );
    await tester.pumpWidget(CubagemCaminhaoApp(api: api));
    await tester.pumpAndSettle();

    expect(find.text('Cubagem de caminhões'), findsOneWidget);
    expect(find.text('Sua frota começa aqui'), findsOneWidget);
    expect(find.text('Cadastrar caminhão'), findsOneWidget);
  });
}

http.Response httpResponse(String body) =>
    http.Response(body, 200, headers: {'content-type': 'application/json'});
