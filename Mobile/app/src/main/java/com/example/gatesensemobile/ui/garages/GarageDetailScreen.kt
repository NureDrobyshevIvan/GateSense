package com.example.gatesensemobile.ui.garages

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.automirrored.filled.List
import androidx.compose.material.icons.filled.LockOpen
import androidx.compose.material.icons.filled.Lock
import androidx.compose.material.icons.filled.People
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material.icons.filled.Warning
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.compose.viewModel

class GarageDetailVMFactory(private val garageId: Int) : ViewModelProvider.Factory {
    @Suppress("UNCHECKED_CAST")
    override fun <T : ViewModel> create(modelClass: Class<T>): T =
        GarageDetailViewModel(garageId) as T
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun GarageDetailScreen(
    garageId: Int,
    onBack: () -> Unit,
    onOpenAccess: (Int) -> Unit,
    onOpenEvents: (Int) -> Unit,
    vm: GarageDetailViewModel = viewModel(factory = GarageDetailVMFactory(garageId))
) {
    val state by vm.state.collectAsState()
    val isOpen = state.gateState?.state.equals("Open", true)

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(state.garage?.name ?: "Гараж") },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = null)
                    }
                },
                actions = {
                    IconButton(onClick = { vm.refreshAll() }) {
                        Icon(Icons.Filled.Refresh, contentDescription = null)
                    }
                }
            )
        }
    ) { padding ->
        Column(
            Modifier.padding(padding).padding(16.dp).fillMaxSize().verticalScroll(rememberScrollState()),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            if (state.alerts.isNotEmpty()) {
                Card(
                    colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.errorContainer),
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Row(Modifier.padding(12.dp), verticalAlignment = Alignment.CenterVertically) {
                        Icon(Icons.Filled.Warning, contentDescription = null, tint = MaterialTheme.colorScheme.onErrorContainer)
                        Spacer(Modifier.width(8.dp))
                        Text(
                            "Активна тривога: ${state.alerts.joinToString { "${it.sensorTypeLabel}=${it.value}" }}",
                            color = MaterialTheme.colorScheme.onErrorContainer
                        )
                    }
                }
                Spacer(Modifier.height(16.dp))
            }

            // Big gate button
            Surface(
                shape = CircleShape,
                color = if (isOpen) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant,
                tonalElevation = 4.dp,
                modifier = Modifier.size(220.dp)
            ) {
                Box(contentAlignment = Alignment.Center) {
                    if (state.acting) {
                        CircularProgressIndicator()
                    } else {
                        IconButton(onClick = { vm.toggleGate() }, modifier = Modifier.size(180.dp)) {
                            Icon(
                                if (isOpen) Icons.Filled.LockOpen else Icons.Filled.Lock,
                                contentDescription = null,
                                modifier = Modifier.size(96.dp)
                            )
                        }
                    }
                }
            }
            Spacer(Modifier.height(12.dp))
            Text(
                state.gateState?.state ?: "—",
                style = MaterialTheme.typography.headlineSmall,
                fontWeight = FontWeight.SemiBold
            )
            state.gateState?.lastActionTime?.let {
                Text("Останнє: $it", style = MaterialTheme.typography.bodySmall, color = Color.Gray)
            }

            state.error?.let {
                Spacer(Modifier.height(12.dp))
                Text(it, color = MaterialTheme.colorScheme.error)
            }

            Spacer(Modifier.height(24.dp))

            // Sensor readings
            ElevatedCard(Modifier.fillMaxWidth()) {
                Column(Modifier.padding(16.dp)) {
                    Text("Останні показники сенсорів", style = MaterialTheme.typography.titleMedium)
                    Spacer(Modifier.height(8.dp))
                    if (state.latestReadings.isEmpty()) {
                        Text("Дані ще не надходили", style = MaterialTheme.typography.bodySmall)
                    } else {
                        state.latestReadings.forEach { r ->
                            Row(Modifier.fillMaxWidth().padding(vertical = 4.dp)) {
                                Text(r.sensorTypeLabel, Modifier.weight(1f))
                                Text("${r.value} ${r.unit ?: ""}")
                            }
                        }
                    }
                }
            }

            Spacer(Modifier.height(16.dp))

            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedButton(onClick = { onOpenEvents(garageId) }, modifier = Modifier.weight(1f)) {
                    Icon(Icons.AutoMirrored.Filled.List, contentDescription = null)
                    Spacer(Modifier.width(8.dp))
                    Text("Журнал")
                }
                OutlinedButton(onClick = { onOpenAccess(garageId) }, modifier = Modifier.weight(1f)) {
                    Icon(Icons.Filled.People, contentDescription = null)
                    Spacer(Modifier.width(8.dp))
                    Text("Доступи")
                }
            }
        }
    }
}
