package com.example.gatesensemobile.ui.garages

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.ChevronRight
import androidx.compose.material.icons.filled.Logout
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun GaragesScreen(
    onOpen: (Int) -> Unit,
    onLoggedOut: () -> Unit,
    vm: GaragesViewModel = viewModel()
) {
    val state by vm.state.collectAsState()
    var showCreateDialog by remember { mutableStateOf(false) }

    LaunchedEffect(state.justLoggedOut) {
        if (state.justLoggedOut) onLoggedOut()
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(if (state.userName != null) "Привіт, ${state.userName}" else "Гаражі") },
                actions = {
                    IconButton(onClick = { vm.logout() }) {
                        Icon(Icons.Filled.Logout, contentDescription = "Вийти")
                    }
                }
            )
        },
        floatingActionButton = {
            FloatingActionButton(onClick = { showCreateDialog = true }) {
                Icon(Icons.Filled.Add, contentDescription = "Додати гараж")
            }
        }
    ) { padding ->
        Box(Modifier.padding(padding).fillMaxSize()) {
            when {
                state.loading && state.items.isEmpty() ->
                    CircularProgressIndicator(Modifier.align(Alignment.Center))
                state.error != null && state.items.isEmpty() ->
                    Column(
                        modifier = Modifier.align(Alignment.Center).padding(24.dp),
                        horizontalAlignment = Alignment.CenterHorizontally
                    ) {
                        Text(state.error!!, color = MaterialTheme.colorScheme.error)
                        Spacer(Modifier.height(8.dp))
                        Button(onClick = { vm.refresh() }) { Text("Повторити") }
                    }
                state.items.isEmpty() ->
                    Text(
                        "Поки що немає жодного гаража.\nНатисніть «+», щоб додати.",
                        modifier = Modifier.align(Alignment.Center).padding(24.dp)
                    )
                else -> LazyColumn(
                    contentPadding = PaddingValues(12.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    items(state.items, key = { it.id }) { g ->
                        ElevatedCard(onClick = { onOpen(g.id) }) {
                            Row(
                                Modifier.padding(16.dp).fillMaxWidth(),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column(Modifier.weight(1f)) {
                                    Text(g.name, style = MaterialTheme.typography.titleMedium)
                                    g.address?.let { Text(it, style = MaterialTheme.typography.bodySmall) }
                                }
                                Icon(Icons.Filled.ChevronRight, contentDescription = null)
                            }
                        }
                    }
                }
            }
        }
    }

    if (showCreateDialog) {
        CreateGarageDialog(
            busy = state.createBusy,
            onDismiss = { showCreateDialog = false },
            onCreate = { name, addr ->
                vm.createGarage(name, addr) { showCreateDialog = false }
            }
        )
    }
}

@Composable
private fun CreateGarageDialog(
    busy: Boolean,
    onDismiss: () -> Unit,
    onCreate: (String, String?) -> Unit
) {
    var name by remember { mutableStateOf("") }
    var address by remember { mutableStateOf("") }
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Новий гараж") },
        text = {
            Column {
                OutlinedTextField(name, { name = it }, label = { Text("Назва") }, singleLine = true, modifier = Modifier.fillMaxWidth())
                Spacer(Modifier.height(8.dp))
                OutlinedTextField(address, { address = it }, label = { Text("Адреса (опц.)") }, singleLine = true, modifier = Modifier.fillMaxWidth())
            }
        },
        confirmButton = {
            TextButton(enabled = !busy, onClick = { onCreate(name, address) }) {
                Text(if (busy) "..." else "Створити")
            }
        },
        dismissButton = { TextButton(onClick = onDismiss) { Text("Скасувати") } }
    )
}
