
import React, {useState} from 'react';
import {
  ActivityIndicator,
  Alert,
  FlatList,
  Pressable,
  SafeAreaView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import type { SelectedConnection } from '../navigation/types';

type Stop = {
  id: number;
  name: string;
};

type Connection = {
  externalRunId: number;
  externalVariantId: number;
  lineNumber: string;
  fromStopId: number;
  fromStopName: string;
  toStopId: number;
  toStopName: string;
  departureTime: string;
  arrivalTime: string;
  durationMinutes: number;
  scheduledTripId: number;
  serviceDate: string;
};

type SearchResponse = {
  mode: string;
  serviceDate: string;
  count: number;
  connections: Connection[];
};

type Props = {
  apiBaseUrl: string;
  onSelectConnection?: (connection: SelectedConnection) => void;
};

// Przystanki potwierdzone w bazie VeloCity.
// Później zastąpimy je listą pobieraną z API.
const STOPS: Stop[] = [
  {id: 178, name: 'Wyspiańskiego - MPK'},
  {id: 130, name: '29 Listopada'},
  {id: 334, name: '29 Listopada - Przychodnia'},
  {id: 134, name: '29 Listopada - Szkoła'},
  {id: 266, name: 'Nawojowska - Traugutta'},
  {id: 289, name: 'Dw. Aut. MPK'},
];

export default function ConnectionSearchScreen({
  apiBaseUrl,
  onSelectConnection,
}: Props) {
  const [fromIndex, setFromIndex] = useState(0);
  const [toIndex, setToIndex] = useState(STOPS.length - 1);
  const [connections, setConnections] = useState<Connection[]>([]);
  const [loading, setLoading] = useState(false);
  const [searched, setSearched] = useState(false);
  const [serviceDate, setServiceDate] = useState<string>('');

  const from = STOPS[fromIndex];
  const to = STOPS[toIndex];

  const changeStop = (
    index: number,
    setter: (value: number) => void,
  ) => {
    setter((index + 1) % STOPS.length);
    setConnections([]);
    setServiceDate('');
    setSearched(false);
  };

  const search = async () => {
    if (from.id === to.id) {
      Alert.alert('Błąd', 'Wybierz dwa różne przystanki.');
      return;
    }

    setLoading(true);
    setSearched(false);
    setConnections([]);

    try {
      const baseUrl = apiBaseUrl.replace(/\/+$/, '');
      const url =
        `${baseUrl}/api/connections/search` +
        `?fromStopId=${from.id}&toStopId=${to.id}`;

      const response = await fetch(url);

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      const data: SearchResponse = await response.json();

        setServiceDate(data.serviceDate);
        setConnections(data.connections);
        setSearched(true);
    } catch (error) {
      Alert.alert(
        'Błąd wyszukiwania',
        error instanceof Error
          ? error.message
          : 'Nie udało się pobrać połączeń.',
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <Text style={styles.title}>Wyszukaj połączenie</Text>
      <Text style={styles.subtitle}>
        Linia 6 · Rozkład demonstracyjny
      </Text>

      <Text style={styles.label}>Przystanek początkowy</Text>
      <Pressable
        style={styles.stopButton}
        onPress={() => changeStop(fromIndex, setFromIndex)}>
        <Text style={styles.stopText}>{from.name}</Text>
        <Text style={styles.hint}>Dotknij, aby zmienić</Text>
      </Pressable>

      <Text style={styles.label}>Przystanek docelowy</Text>
      <Pressable
        style={styles.stopButton}
        onPress={() => changeStop(toIndex, setToIndex)}>
        <Text style={styles.stopText}>{to.name}</Text>
        <Text style={styles.hint}>Dotknij, aby zmienić</Text>
      </Pressable>

      <Pressable
        style={styles.searchButton}
        onPress={search}
        disabled={loading}>
        <Text style={styles.searchText}>Szukaj połączeń</Text>
      </Pressable>

      {loading && <ActivityIndicator size="large" />}

      {searched && (
        <Text style={styles.resultsTitle}>
          Znalezione połączenia: {connections.length}
        </Text>
      )}

      <FlatList
        data={connections}
        keyExtractor={item => String(item.externalRunId)}
        contentContainerStyle={styles.results}
        ListEmptyComponent={
          searched ? (
            <Text style={styles.empty}>
              Brak połączeń dla wybranych przystanków.
            </Text>
          ) : null
        }
        renderItem={({item}) => (
          <Pressable
            style={styles.card}
            onPress={() =>
                onSelectConnection?.({
                    ...item,
                    serviceDate,
                })
                }>
            <View style={styles.cardHeader}>
              <Text style={styles.line}>Linia 6</Text>
              <Text style={styles.duration}>
                {item.durationMinutes} min
              </Text>
            </View>

            <Text style={styles.hours}>
              {item.departureTime} → {item.arrivalTime}
            </Text>

            <Text style={styles.route}>
              {item.fromStopName} → {item.toStopName}
            </Text>

            <Text style={styles.run}>
              Kurs MPK: {item.externalRunId}
            </Text>

            {onSelectConnection && (
              <Text style={styles.select}>Wybierz połączenie</Text>
            )}
          </Pressable>
        )}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F5F7FA',
    padding: 16,
  },
  title: {
    fontSize: 24,
    fontWeight: '700',
    color: '#172033',
  },
  subtitle: {
    color: '#64748B',
    marginBottom: 20,
    marginTop: 4,
  },
  label: {
    fontSize: 14,
    fontWeight: '600',
    marginBottom: 6,
    color: '#334155',
  },
  stopButton: {
    backgroundColor: '#FFFFFF',
    padding: 15,
    borderRadius: 12,
    marginBottom: 16,
    borderWidth: 1,
    borderColor: '#E2E8F0',
  },
  stopText: {
    fontSize: 16,
    color: '#172033',
    fontWeight: '600',
  },
  hint: {
    fontSize: 12,
    color: '#64748B',
    marginTop: 5,
  },
  searchButton: {
    backgroundColor: '#1769E0',
    padding: 16,
    borderRadius: 12,
    alignItems: 'center',
    marginBottom: 20,
  },
  searchText: {
    color: '#FFFFFF',
    fontWeight: '700',
    fontSize: 16,
  },
  resultsTitle: {
    fontWeight: '700',
    fontSize: 17,
    marginBottom: 12,
    color: '#172033',
  },
  results: {
    paddingBottom: 30,
  },
  card: {
    backgroundColor: '#FFFFFF',
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: '#E2E8F0',
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  line: {
    fontWeight: '700',
    color: '#1769E0',
  },
  duration: {
    color: '#64748B',
  },
  hours: {
    fontSize: 23,
    fontWeight: '700',
    color: '#172033',
    marginVertical: 8,
  },
  route: {
    color: '#334155',
  },
  run: {
    fontSize: 12,
    color: '#64748B',
    marginTop: 8,
  },
  select: {
    color: '#1769E0',
    fontWeight: '700',
    marginTop: 12,
  },
  empty: {
    color: '#64748B',
    textAlign: 'center',
    marginTop: 16,
  },
});
