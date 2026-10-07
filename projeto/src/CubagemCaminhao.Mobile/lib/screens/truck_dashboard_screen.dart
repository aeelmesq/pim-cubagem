import 'package:flutter/material.dart';

import '../models/truck.dart';
import '../services/truck_api.dart';

class TruckDashboardScreen extends StatefulWidget {
  const TruckDashboardScreen({required this.api, super.key});

  final TruckApi api;

  @override
  State<TruckDashboardScreen> createState() => _TruckDashboardScreenState();
}

class _TruckDashboardScreenState extends State<TruckDashboardScreen> {
  late Future<List<Truck>> _trucksFuture;

  @override
  void initState() {
    super.initState();
    _trucksFuture = widget.api.getTrucks();
  }

  Future<void> _reload() async {
    setState(() {
      _trucksFuture = widget.api.getTrucks();
    });
    await _trucksFuture;
  }

  Future<void> _showCreateTruckDialog() async {
    final truckInput = await showDialog<TruckInput>(
      context: context,
      builder: (_) => const TruckFormDialog(),
    );
    if (truckInput == null || !mounted) {
      return;
    }

    await _runMutation(
      () => widget.api.createTruck(
        name: truckInput.name,
        plate: truckInput.plate,
        lengthCm: truckInput.lengthCm,
        widthCm: truckInput.widthCm,
        heightCm: truckInput.heightCm,
      ),
      successMessage: 'Caminhão cadastrado.',
    );
  }

  Future<void> _showCreateCargoDialog(Truck truck) async {
    final cargoInput = await showDialog<CargoInput>(
      context: context,
      builder: (_) => CargoFormDialog(truckName: truck.name),
    );
    if (cargoInput == null || !mounted) {
      return;
    }

    await _runMutation(
      () => widget.api.createCargo(
        truckId: truck.id,
        name: cargoInput.name,
        lengthCm: cargoInput.lengthCm,
        widthCm: cargoInput.widthCm,
        heightCm: cargoInput.heightCm,
        quantity: cargoInput.quantity,
      ),
      successMessage: 'Item de carga adicionado.',
    );
  }

  Future<void> _confirmDeleteTruck(Truck truck) async {
    final confirmed = await _confirm(
      title: 'Excluir caminhão?',
      message:
          'O caminhão ${truck.name} e os itens de carga vinculados serão excluídos.',
    );
    if (!confirmed || !mounted) {
      return;
    }

    await _runMutation(
      () => widget.api.deleteTruck(truck.id),
      successMessage: 'Caminhão excluído.',
    );
  }

  Future<void> _confirmDeleteCargo(Truck truck, CargoItem cargo) async {
    final confirmed = await _confirm(
      title: 'Remover item?',
      message: 'Remover ${cargo.name} da carga de ${truck.name}?',
    );
    if (!confirmed || !mounted) {
      return;
    }

    await _runMutation(
      () => widget.api.deleteCargo(truckId: truck.id, cargoId: cargo.id),
      successMessage: 'Item removido da carga.',
    );
  }

  Future<bool> _confirm({
    required String title,
    required String message,
  }) async {
    return await showDialog<bool>(
          context: context,
          builder: (context) => AlertDialog(
            title: Text(title),
            content: Text(message),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(context, false),
                child: const Text('Cancelar'),
              ),
              FilledButton(
                onPressed: () => Navigator.pop(context, true),
                child: const Text('Confirmar'),
              ),
            ],
          ),
        ) ??
        false;
  }

  Future<void> _runMutation(
    Future<void> Function() mutation, {
    required String successMessage,
  }) async {
    try {
      await mutation();
      if (!mounted) {
        return;
      }
      await _reload();
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text(successMessage)));
    } catch (error) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(error.toString()),
          backgroundColor: Theme.of(context).colorScheme.error,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Cubagem de caminhões'),
        actions: [
          IconButton(
            tooltip: 'Atualizar',
            onPressed: _reload,
            icon: const Icon(Icons.refresh),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _showCreateTruckDialog,
        icon: const Icon(Icons.add),
        label: const Text('Caminhão'),
      ),
      body: FutureBuilder<List<Truck>>(
        future: _trucksFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return _ErrorView(
              message: snapshot.error.toString(),
              onRetry: _reload,
            );
          }

          final trucks = snapshot.data ?? const <Truck>[];
          if (trucks.isEmpty) {
            return _EmptyView(onAddTruck: _showCreateTruckDialog);
          }

          return RefreshIndicator(
            onRefresh: _reload,
            child: ListView(
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 96),
              children: [
                _SummaryCard(trucks: trucks),
                const SizedBox(height: 20),
                Text(
                  'Frota cadastrada',
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                const SizedBox(height: 8),
                ...trucks.map(
                  (truck) => Padding(
                    padding: const EdgeInsets.only(bottom: 12),
                    child: _TruckCard(
                      truck: truck,
                      onAddCargo: () => _showCreateCargoDialog(truck),
                      onDeleteTruck: () => _confirmDeleteTruck(truck),
                      onDeleteCargo: (cargo) =>
                          _confirmDeleteCargo(truck, cargo),
                    ),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}

class _SummaryCard extends StatelessWidget {
  const _SummaryCard({required this.trucks});

  final List<Truck> trucks;

  @override
  Widget build(BuildContext context) {
    final totalCargoVolume = trucks.fold<double>(
      0,
      (total, truck) => total + truck.cargoVolumeM3,
    );
    final averageOccupancy = trucks.isEmpty
        ? 0.0
        : trucks.fold<double>(
                0,
                (total, truck) => total + truck.occupancyPercent,
              ) /
              trucks.length;

    return Card(
      color: Theme.of(context).colorScheme.primary,
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Resumo da frota',
              style: Theme.of(context).textTheme.titleMedium
                  ?.copyWith(color: Colors.white, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                _SummaryValue(label: 'Caminhões', value: '${trucks.length}'),
                _SummaryValue(
                  label: 'Volume de carga',
                  value: '${totalCargoVolume.toStringAsFixed(2)} m³',
                ),
                _SummaryValue(
                  label: 'Ocupação média',
                  value: '${averageOccupancy.toStringAsFixed(1)}%',
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _SummaryValue extends StatelessWidget {
  const _SummaryValue({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: const TextStyle(color: Colors.white70)),
          const SizedBox(height: 4),
          Text(
            value,
            style: Theme.of(context).textTheme.titleMedium
                ?.copyWith(color: Colors.white, fontWeight: FontWeight.bold),
          ),
        ],
      ),
    );
  }
}

class _TruckCard extends StatelessWidget {
  const _TruckCard({
    required this.truck,
    required this.onAddCargo,
    required this.onDeleteTruck,
    required this.onDeleteCargo,
  });

  final Truck truck;
  final VoidCallback onAddCargo;
  final VoidCallback onDeleteTruck;
  final ValueChanged<CargoItem> onDeleteCargo;

  @override
  Widget build(BuildContext context) {
    final occupancy = truck.occupancyPercent;
    final occupancyColor = occupancy >= 80
        ? Theme.of(context).colorScheme.error
        : occupancy >= 60
        ? Colors.orange.shade800
        : Theme.of(context).colorScheme.primary;

    return Card(
      clipBehavior: Clip.antiAlias,
      child: ExpansionTile(
        leading: const CircleAvatar(child: Icon(Icons.local_shipping_outlined)),
        title: Text(
          truck.name,
          style: const TextStyle(fontWeight: FontWeight.w600),
        ),
        subtitle: Text(truck.plate),
        trailing: Text(
          '${occupancy.toStringAsFixed(1)}%',
          style: TextStyle(color: occupancyColor, fontWeight: FontWeight.bold),
        ),
        childrenPadding: const EdgeInsets.fromLTRB(16, 0, 16, 12),
        children: [
          LinearProgressIndicator(
            value: (occupancy / 100).clamp(0.0, 1.0),
            color: occupancyColor,
            minHeight: 7,
            borderRadius: BorderRadius.circular(8),
          ),
          const SizedBox(height: 12),
          Align(
            alignment: Alignment.centerLeft,
            child: Text(
              'Dimensões: ${_formatDimension(truck.lengthCm)} × '
              '${_formatDimension(truck.widthCm)} × '
              '${_formatDimension(truck.heightCm)} cm\n'
              'Volume: ${truck.cargoVolumeM3.toStringAsFixed(3)} / '
              '${truck.truckVolumeM3.toStringAsFixed(3)} m³',
              style: Theme.of(context).textTheme.bodyMedium,
            ),
          ),
          const SizedBox(height: 8),
          if (truck.cargoItems.isEmpty)
            const Padding(
              padding: EdgeInsets.symmetric(vertical: 12),
              child: Text('Nenhum item de carga cadastrado.'),
            )
          else
            ...truck.cargoItems.map(
              (cargo) => ListTile(
                contentPadding: EdgeInsets.zero,
                title: Text(cargo.name),
                subtitle: Text(
                  '${cargo.quantity} un. • '
                  '${_formatDimension(cargo.lengthCm)} × '
                  '${_formatDimension(cargo.widthCm)} × '
                  '${_formatDimension(cargo.heightCm)} cm • '
                  '${cargo.totalVolumeM3.toStringAsFixed(3)} m³',
                ),
                trailing: IconButton(
                  tooltip: 'Remover item',
                  onPressed: () => onDeleteCargo(cargo),
                  icon: const Icon(Icons.delete_outline),
                ),
              ),
            ),
          const SizedBox(height: 4),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              OutlinedButton.icon(
                onPressed: onAddCargo,
                icon: const Icon(Icons.add_box_outlined),
                label: const Text('Adicionar carga'),
              ),
              TextButton.icon(
                onPressed: onDeleteTruck,
                icon: const Icon(Icons.delete_outline),
                label: const Text('Excluir caminhão'),
              ),
            ],
          ),
        ],
      ),
    );
  }

  String _formatDimension(double value) => value == value.roundToDouble()
      ? value.toStringAsFixed(0)
      : value.toString();
}

class _ErrorView extends StatelessWidget {
  const _ErrorView({required this.message, required this.onRetry});

  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.cloud_off_outlined,
              size: 48,
              color: Theme.of(context).colorScheme.error,
            ),
            const SizedBox(height: 12),
            Text(
              'Não foi possível carregar os caminhões.',
              style: Theme.of(context).textTheme.titleMedium,
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 8),
            Text(message, textAlign: TextAlign.center),
            const SizedBox(height: 16),
            FilledButton.icon(
              onPressed: onRetry,
              icon: const Icon(Icons.refresh),
              label: const Text('Tentar novamente'),
            ),
          ],
        ),
      ),
    );
  }
}

class _EmptyView extends StatelessWidget {
  const _EmptyView({required this.onAddTruck});

  final VoidCallback onAddTruck;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.local_shipping_outlined,
              size: 56,
              color: Theme.of(context).colorScheme.primary,
            ),
            const SizedBox(height: 12),
            Text(
              'Sua frota começa aqui',
              style: Theme.of(context).textTheme.titleLarge,
            ),
            const SizedBox(height: 8),
            const Text(
              'Cadastre um caminhão para acompanhar a cubagem e as cargas.',
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 16),
            FilledButton.icon(
              onPressed: onAddTruck,
              icon: const Icon(Icons.add),
              label: const Text('Cadastrar caminhão'),
            ),
          ],
        ),
      ),
    );
  }
}

class TruckInput {
  const TruckInput({
    required this.name,
    required this.plate,
    required this.lengthCm,
    required this.widthCm,
    required this.heightCm,
  });

  final String name;
  final String plate;
  final double lengthCm;
  final double widthCm;
  final double heightCm;
}

class CargoInput {
  const CargoInput({
    required this.name,
    required this.lengthCm,
    required this.widthCm,
    required this.heightCm,
    required this.quantity,
  });

  final String name;
  final double lengthCm;
  final double widthCm;
  final double heightCm;
  final int quantity;
}

class TruckFormDialog extends StatefulWidget {
  const TruckFormDialog({super.key});

  @override
  State<TruckFormDialog> createState() => _TruckFormDialogState();
}

class _TruckFormDialogState extends State<TruckFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _plateController = TextEditingController();
  final _lengthController = TextEditingController(text: '500');
  final _widthController = TextEditingController(text: '220');
  final _heightController = TextEditingController(text: '240');

  @override
  void dispose() {
    _nameController.dispose();
    _plateController.dispose();
    _lengthController.dispose();
    _widthController.dispose();
    _heightController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Cadastrar caminhão'),
      content: Form(
        key: _formKey,
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                controller: _nameController,
                decoration: const InputDecoration(labelText: 'Nome'),
                textCapitalization: TextCapitalization.words,
                validator: _requiredText,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _plateController,
                decoration: const InputDecoration(labelText: 'Placa'),
                textCapitalization: TextCapitalization.characters,
                validator: _requiredText,
              ),
              const SizedBox(height: 12),
              _numberField(_lengthController, 'Comprimento (cm)'),
              const SizedBox(height: 12),
              _numberField(_widthController, 'Largura (cm)'),
              const SizedBox(height: 12),
              _numberField(_heightController, 'Altura (cm)'),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Cancelar'),
        ),
        FilledButton(onPressed: _submit, child: const Text('Salvar')),
      ],
    );
  }

  Widget _numberField(TextEditingController controller, String label) {
    return TextFormField(
      controller: controller,
      decoration: InputDecoration(labelText: label),
      keyboardType: const TextInputType.numberWithOptions(decimal: true),
      validator: _positiveNumber,
    );
  }

  void _submit() {
    if (!_formKey.currentState!.validate()) {
      return;
    }
    Navigator.pop(
      context,
      TruckInput(
        name: _nameController.text.trim(),
        plate: _plateController.text.trim().toUpperCase(),
        lengthCm: _parseNumber(_lengthController.text),
        widthCm: _parseNumber(_widthController.text),
        heightCm: _parseNumber(_heightController.text),
      ),
    );
  }
}

class CargoFormDialog extends StatefulWidget {
  const CargoFormDialog({required this.truckName, super.key});

  final String truckName;

  @override
  State<CargoFormDialog> createState() => _CargoFormDialogState();
}

class _CargoFormDialogState extends State<CargoFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _lengthController = TextEditingController(text: '100');
  final _widthController = TextEditingController(text: '100');
  final _heightController = TextEditingController(text: '100');
  final _quantityController = TextEditingController(text: '1');

  @override
  void dispose() {
    _nameController.dispose();
    _lengthController.dispose();
    _widthController.dispose();
    _heightController.dispose();
    _quantityController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Adicionar item de carga'),
      content: Form(
        key: _formKey,
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Align(
                alignment: Alignment.centerLeft,
                child: Text('Caminhão: ${widget.truckName}'),
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _nameController,
                decoration: const InputDecoration(labelText: 'Produto/carga'),
                textCapitalization: TextCapitalization.words,
                validator: _requiredText,
              ),
              const SizedBox(height: 12),
              _numberField(_lengthController, 'Comprimento (cm)'),
              const SizedBox(height: 12),
              _numberField(_widthController, 'Largura (cm)'),
              const SizedBox(height: 12),
              _numberField(_heightController, 'Altura (cm)'),
              const SizedBox(height: 12),
              TextFormField(
                controller: _quantityController,
                decoration: const InputDecoration(labelText: 'Quantidade'),
                keyboardType: TextInputType.number,
                validator: _positiveInteger,
              ),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Cancelar'),
        ),
        FilledButton(onPressed: _submit, child: const Text('Adicionar')),
      ],
    );
  }

  Widget _numberField(TextEditingController controller, String label) {
    return TextFormField(
      controller: controller,
      decoration: InputDecoration(labelText: label),
      keyboardType: const TextInputType.numberWithOptions(decimal: true),
      validator: _positiveNumber,
    );
  }

  void _submit() {
    if (!_formKey.currentState!.validate()) {
      return;
    }
    Navigator.pop(
      context,
      CargoInput(
        name: _nameController.text.trim(),
        lengthCm: _parseNumber(_lengthController.text),
        widthCm: _parseNumber(_widthController.text),
        heightCm: _parseNumber(_heightController.text),
        quantity: int.parse(_quantityController.text.trim()),
      ),
    );
  }
}

String? _requiredText(String? value) {
  if (value == null || value.trim().isEmpty) {
    return 'Campo obrigatório.';
  }
  return null;
}

String? _positiveNumber(String? value) {
  if (value == null || value.trim().isEmpty) {
    return 'Informe uma dimensão.';
  }
  final number = _tryParseNumber(value);
  if (number == null || number <= 0 || number >= 100000) {
    return 'Informe um número maior que 0 e menor que 100.000.';
  }
  return null;
}

String? _positiveInteger(String? value) {
  final quantity = int.tryParse(value?.trim() ?? '');
  if (quantity == null || quantity < 1 || quantity > 100000) {
    return 'Informe uma quantidade entre 1 e 100.000.';
  }
  return null;
}

double? _tryParseNumber(String value) =>
    double.tryParse(value.trim().replaceAll(',', '.'));

double _parseNumber(String value) => _tryParseNumber(value)!;
