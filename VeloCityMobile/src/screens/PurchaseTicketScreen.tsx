import React, { useState, useEffect } from 'react';
import {
  StyleSheet,
  View,
  Text,
  FlatList,
  TouchableOpacity,
  Alert,
} from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import Icon from '@react-native-vector-icons/ionicons';

import apiService from '../api/apiService';
import { RootStackParamList } from '../navigation/types';
import type { PurchaseTicketCommand, TicketTypeDto } from '../types/ticket';
import LoadingOverlay from '../components/LoadingOverlay';
import { translateApiError } from '../utils/errorTranslator';

type Props = NativeStackScreenProps<RootStackParamList, 'PurchaseTicket'>;

const PurchaseTicketScreen = ({navigation, route}: Props) => {
  const selectedConnection = route.params?.connection;
  const [ticketTypes, setTicketTypes] = useState<TicketTypeDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [purchasingId, setPurchasingId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchTicketTypes();
  }, []);

  const fetchTicketTypes = async () => {
    try {
      const data = await apiService.getTicketTypes();
      setTicketTypes(data);
    } catch (err: any) {
      setError(translateApiError(err.message));
    } finally {
      setIsLoading(false);
    }
  };

  const handlePurchasePress = (ticket: TicketTypeDto) => {
    Alert.alert(
      'Potwierdzenie zakupu',
      `Czy na pewno chcesz kupić:\n${ticket.name}\n\nKoszt: ${ticket.price.toFixed(2)} PLN`,
      [
        { text: 'Anuluj', style: 'cancel' },
        { 
          text: 'Kupuję', 
          style: 'default',
          onPress: () =>
            executePurchase({
              ticketTypeId: ticket.id,
              ...(selectedConnection
                ? {
                    scheduledTripId: selectedConnection.scheduledTripId,
                    serviceDate: selectedConnection.serviceDate,
                    startStopId: selectedConnection.fromStopId,
                    endStopId: selectedConnection.toStopId,
                  }
                : {}),
            })
        },
      ]
    );
  };


  const executePurchase = async (command: PurchaseTicketCommand) => {
    setPurchasingId(command.ticketTypeId);
    setError(null);
    try {
      await apiService.purchaseTicket(command);
      
      Alert.alert(
        'Sukces!', 
        'Bilet został zakupiony i znajduje się w Twoim portfelu.',
        [{ text: 'OK', onPress: () => navigation.goBack() }]
      );
    } catch (err: any) {
      setError(translateApiError(err.message));
    } finally {
      setPurchasingId(null);
    }
  };

  const renderTicketCard = ({ item }: { item: TicketTypeDto }) => (
    <View style={styles.card}>
      <View style={styles.cardHeader}>
        <View style={styles.iconContainer}>
          <Icon name="ticket-outline" size={24} color="#346699" />
        </View>
        <View style={styles.cardTitleContainer}>
          <Text style={styles.ticketName}>{item.name}</Text>
          <Text style={styles.ticketDetails}>
            {item.durationInMinutes > 0 ? `⏳ ${item.durationInMinutes} min` : '🔄 Jednorazowy'} • 🌐 Strefa {item.zoneLimit}
          </Text>
        </View>
      </View>
      
      <View style={styles.divider} />
      
      <View style={styles.cardFooter}>
        <Text style={styles.price}>{item.price.toFixed(2)} PLN</Text>
        <TouchableOpacity
          style={styles.buyButton}
          onPress={() => handlePurchasePress(item)}
          disabled={purchasingId !== null}
        >
          <Text style={styles.buyButtonText}>Wybierz</Text>
        </TouchableOpacity>
      </View>
    </View>
  );

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <Icon name="arrow-back" size={24} color="#1A1C1E" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Wybierz bilet</Text>
        <View style={{ width: 40 }} />
      </View>

      {selectedConnection && (
        <View style={styles.connectionCard}>
          <Text style={styles.connectionLabel}>
            WYBRANE POŁĄCZENIE
          </Text>

          <Text style={styles.connectionLine}>
            Linia {selectedConnection.lineNumber}
          </Text>

          <Text style={styles.connectionHours}>
            {selectedConnection.departureTime}
            {'  →  '}
            {selectedConnection.arrivalTime}
          </Text>

          <Text style={styles.connectionRoute}>
            {selectedConnection.fromStopName}
            {'\n↓\n'}
            {selectedConnection.toStopName}
          </Text>

          <Text style={styles.connectionDuration}>
            Czas przejazdu: {selectedConnection.durationMinutes} min
          </Text>

          <Text style={styles.demoNotice}>
            Rozkład demonstracyjny · MPK 86
          </Text>
        </View>
      )}

      {error && (
        <View style={styles.errorContainer}>
          <Text style={styles.errorText}>{error}</Text>
        </View>
      )}

      <FlatList
        data={ticketTypes}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderTicketCard}
        contentContainerStyle={styles.listContent}
        showsVerticalScrollIndicator={false}
        ListEmptyComponent={
          !isLoading ? (
            <Text style={styles.emptyText}>Brak dostępnych biletów w ofercie.</Text>
          ) : null
        }
      />

      <LoadingOverlay visible={isLoading || purchasingId !== null} />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F5F7FA',
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingTop: 50,
    paddingBottom: 16,
    backgroundColor: '#FFF',
    borderBottomWidth: 1,
    borderBottomColor: '#EEF0F2',
  },
  backButton: {
    padding: 8,
  },
  headerTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#1A1C1E',
  },
  listContent: {
    padding: 16,
    paddingBottom: 40,
  },
  card: {
    backgroundColor: '#FFF',
    borderRadius: 16,
    padding: 16,
    marginBottom: 16,
    elevation: 3,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.05,
    shadowRadius: 6,
    borderWidth: 1,
    borderColor: '#EEF0F2',
  },
  cardHeader: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  iconContainer: {
    backgroundColor: '#EBF1F8',
    padding: 12,
    borderRadius: 12,
    marginRight: 16,
  },
  cardTitleContainer: {
    flex: 1,
  },
  ticketName: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#1A1C1E',
    marginBottom: 4,
  },
  ticketDetails: {
    fontSize: 13,
    color: '#666',
  },
  divider: {
    height: 1,
    backgroundColor: '#EEF0F2',
    marginVertical: 16,
    borderStyle: 'dashed',
  },
  cardFooter: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  price: {
    fontSize: 20,
    fontWeight: '900',
    color: '#346699',
  },
  buyButton: {
    backgroundColor: '#346699',
    paddingVertical: 10,
    paddingHorizontal: 24,
    borderRadius: 12,
  },
  buyButtonText: {
    color: '#FFF',
    fontWeight: 'bold',
    fontSize: 14,
  },
  errorContainer: {
    backgroundColor: '#FFE5E5',
    padding: 12,
    margin: 16,
    borderRadius: 8,
    borderLeftWidth: 4,
    borderLeftColor: '#FF3B30',
  },
  errorText: {
    color: '#D92D20',
    fontSize: 14,
  },
  emptyText: {
    textAlign: 'center',
    color: '#666',
    marginTop: 40,
    fontSize: 16,
  },
  connectionCard: {
    backgroundColor: '#EBF1F8',
    padding: 16,
    margin: 16,
    marginBottom: 0,
    borderRadius: 16,
    borderWidth: 1,
    borderColor: '#CAD9EA',
  },
  connectionLabel: {
    fontSize: 11,
    fontWeight: '700',
    color: '#64748B',
    marginBottom: 8,
  },
  connectionLine: {
    fontSize: 17,
    fontWeight: '700',
    color: '#346699',
  },
  connectionHours: {
    fontSize: 24,
    fontWeight: '800',
    color: '#172033',
    marginVertical: 8,
  },
  connectionRoute: {
    fontSize: 14,
    color: '#334155',
    lineHeight: 22,
  },
  connectionDuration: {
    fontSize: 13,
    color: '#64748B',
    marginTop: 8,
  },
  demoNotice: {
    fontSize: 12,
    color: '#64748B',
    marginTop: 10,
    fontStyle: 'italic',
  },
});

export default PurchaseTicketScreen;
