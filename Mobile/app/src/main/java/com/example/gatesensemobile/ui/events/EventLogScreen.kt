package com.example.gatesensemobile.ui.events

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.compose.viewModel

class EventLogVMFactory(private val garageId: Int) : ViewModelProvider.Factory {
    @Suppress("UNCHECKED_CAST")
    override fun <T : ViewModel> create(modelClass: Class<T>): T = EventLogViewModel(garageId) as T
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun EventLogScreen(
    garageId: Int,
    onBack: () -> Unit,
    vm: EventLogViewModel = viewModel(factory = EventLogVMFactory(garageId))
) {
    val state by vm.state.collectAsState()
    Scaffold(topBar = {
        TopAppBar(
            title = { Text("Журнал подій") },
            navigationIcon = {
                IconButton(onClick = onBack) {
                    Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = null)
                }
            },
            actions = {
                IconButton(onClick = { vm.refresh() }) {
                    Icon(Icons.Filled.Refresh, contentDescription = null)
                }
            }
        )
    }) { padding ->
        Box(Modifier.padding(padding).fillMaxSize()) {
            when {
                state.loading -> CircularProgressIndicator(Modifier.align(Alignment.Center))
                state.error != null -> Text(state.error!!, Modifier.align(Alignment.Center), color = MaterialTheme.colorScheme.error)
                state.events.isEmpty() -> Text("Подій ще не було", Modifier.align(Alignment.Center))
                else -> LazyColumn(contentPadding = PaddingValues(12.dp), verticalArrangement = Arrangement.spacedBy(6.dp)) {
                    items(state.events, key = { it.id }) { e ->
                        Card(Modifier.fillMaxWidth()) {
                            Column(Modifier.padding(12.dp)) {
                                Text("${e.actionLabel} • ${e.triggerSourceLabel}", style = MaterialTheme.typography.titleSmall)
                                e.createdOn?.let { Text(it, style = MaterialTheme.typography.bodySmall) }
                                Text("Результат: ${e.resultLabel}", style = MaterialTheme.typography.bodySmall)
                                e.failureReason?.let { Text("Помилка: $it", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.error) }
                            }
                        }
                    }
                }
            }
        }
    }
}
