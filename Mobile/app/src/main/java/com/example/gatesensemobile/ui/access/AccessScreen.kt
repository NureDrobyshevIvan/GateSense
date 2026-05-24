package com.example.gatesensemobile.ui.access

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewmodel.compose.viewModel

class AccessVMFactory(private val garageId: Int) : ViewModelProvider.Factory {
    @Suppress("UNCHECKED_CAST")
    override fun <T : ViewModel> create(modelClass: Class<T>): T = AccessViewModel(garageId) as T
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AccessScreen(
    garageId: Int,
    onBack: () -> Unit,
    vm: AccessViewModel = viewModel(factory = AccessVMFactory(garageId))
) {
    val state by vm.state.collectAsState()
    var showFamily by remember { mutableStateOf(false) }
    var showGuest by remember { mutableStateOf(false) }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Керування доступом") },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = null)
                    }
                }
            )
        }
    ) { padding ->
        Column(Modifier.padding(padding).padding(16.dp).fillMaxSize()) {

            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                Button(onClick = { showFamily = true }, modifier = Modifier.weight(1f)) { Text("+ Сім'я") }
                Button(onClick = { showGuest = true }, modifier = Modifier.weight(1f)) { Text("+ Гість") }
            }

            state.error?.let {
                Spacer(Modifier.height(12.dp))
                Text(it, color = MaterialTheme.colorScheme.error)
            }

            state.lastGuestToken?.let { token ->
                Spacer(Modifier.height(12.dp))
                ElevatedCard(Modifier.fillMaxWidth()) {
                    Column(Modifier.padding(12.dp)) {
                        Text("Гостьовий токен створено:", style = MaterialTheme.typography.titleSmall)
                        Spacer(Modifier.height(4.dp))
                        SelectionContainer { Text(token, style = MaterialTheme.typography.bodySmall) }
                        Spacer(Modifier.height(8.dp))
                        TextButton(onClick = { vm.clearGuestToken() }) { Text("Закрити") }
                    }
                }
            }

            Spacer(Modifier.height(16.dp))
            Text("Учасники", style = MaterialTheme.typography.titleMedium)
            Spacer(Modifier.height(8.dp))

            if (state.loading) {
                Box(Modifier.fillMaxWidth(), contentAlignment = Alignment.Center) { CircularProgressIndicator() }
            } else if (state.members.isEmpty()) {
                Text("Поки що нікого", style = MaterialTheme.typography.bodySmall)
            } else {
                LazyColumn(verticalArrangement = Arrangement.spacedBy(6.dp)) {
                    items(state.members, key = { it.id }) { m ->
                        Card(Modifier.fillMaxWidth()) {
                            Row(
                                Modifier.padding(12.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column(Modifier.weight(1f)) {
                                    Text("${m.user?.userName ?: m.user?.email ?: "User #${m.userId ?: "?"}"} • ${m.accessLevelLabel}")
                                    m.expiresOn?.let { Text("До: $it", style = MaterialTheme.typography.bodySmall) }
                                }
                                IconButton(onClick = { vm.revoke(m.id) }) {
                                    Icon(Icons.Filled.Delete, contentDescription = "Видалити")
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    if (showFamily) {
        AssignFamilyDialog(
            busy = state.busy,
            onDismiss = { showFamily = false },
            onAssign = {
                vm.assignFamily(it)
                showFamily = false
            }
        )
    }
    if (showGuest) {
        CreateGuestDialog(
            busy = state.busy,
            onDismiss = { showGuest = false },
            onCreate = {
                vm.createGuest(it)
                showGuest = false
            }
        )
    }
}

@Composable
private fun AssignFamilyDialog(busy: Boolean, onDismiss: () -> Unit, onAssign: (String) -> Unit) {
    var email by remember { mutableStateOf("") }
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Додати члена сім'ї") },
        text = {
            OutlinedTextField(email, { email = it }, label = { Text("Email") }, singleLine = true, modifier = Modifier.fillMaxWidth())
        },
        confirmButton = { TextButton(enabled = !busy, onClick = { onAssign(email) }) { Text("Додати") } },
        dismissButton = { TextButton(onClick = onDismiss) { Text("Скасувати") } }
    )
}

@Composable
private fun CreateGuestDialog(busy: Boolean, onDismiss: () -> Unit, onCreate: (String) -> Unit) {
    var name by remember { mutableStateOf("") }
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Створити гостьовий ключ") },
        text = {
            OutlinedTextField(name, { name = it }, label = { Text("Ім'я гостя") }, singleLine = true, modifier = Modifier.fillMaxWidth())
        },
        confirmButton = { TextButton(enabled = !busy, onClick = { onCreate(name) }) { Text("Створити") } },
        dismissButton = { TextButton(onClick = onDismiss) { Text("Скасувати") } }
    )
}

@Composable
private fun SelectionContainer(content: @Composable () -> Unit) {
    androidx.compose.foundation.text.selection.SelectionContainer { content() }
}
