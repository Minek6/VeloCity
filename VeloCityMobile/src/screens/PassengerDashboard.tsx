import React, { useState, useEffect } from 'react';
import Logo from '../assets/logo.png';
import { View, Text, StyleSheet, TouchableOpacity, ScrollView, Image, ActivityIndicator, Modal, Alert,} from 'react-native';
import { useIsFocused } from '@react-navigation/native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useAuthStore } from '../store/useAuthStore';
import apiService from '../api/apiService';
import { TicketDto } from '../types/ticket';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RootStackParamList } from '../navigation/types';
import { Vehicle } from '../types';
import QRCode from 'react-native-qrcode-svg';

interface PassengerDashboardProps {
  navigation: NativeStackNavigationProp<RootStackParamList>; 
}

const PassengerDashboard = ({ navigation }: PassengerDashboardProps) => {
  const { user } = useAuthStore();
  const [balance, setBalance] = useState<string>('0.00');
  const [activeTicket, setActiveTicket] = useState<TicketDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [purchasedTickets, setPurchasedTickets] = useState<TicketDto[]>([]);
  const [selectedTicketId, setSelectedTicketId] = useState<number | null>(null);
  const [selectedVehicleId, setSelectedVehicleId] = useState<number | null>(null);
  const [showVehiclePicker, setShowVehiclePicker] = useState(false);
  const [isActivating, setIsActivating] = useState(false);

  const isFocused = useIsFocused();

  useEffect(() => {
    const fetchDashboardData = async () => {
      setIsLoading(true);
      try {
        const walletResponse = await apiService.getBalance();
        setBalance(Number(walletResponse.balance).toFixed(2));

        const ticketResponse = await apiService.getActiveTicket();
        setActiveTicket(ticketResponse ?? []);

        const vehiclesResponse = await apiService.getVehicles();
        setVehicles(vehiclesResponse ?? []);

        const historyResponse = await apiService.getTicketHistory(1, 100);
        setPurchasedTickets(historyResponse.items.filter(ticket => !ticket.isValidated));
        
      } catch (error) {
        console.error('Błąd podczas pobierania danych pulpitu:', error);
        setActiveTicket([]);
      } finally {
        setIsLoading(false);
      }
    };

    if(isFocused) fetchDashboardData();
  }, [isFocused]);

const handleActivateTicket = async () => {
  if (selectedTicketId === null || selectedVehicleId === null) {
    Alert.alert('Błąd', 'Wybierz pojazd.');
    return;
  }

  if (isActivating) return;

  const ticketId = selectedTicketId;
  const vehicleId = selectedVehicleId;

  setIsActivating(true);

  try {
    await apiService.validateTicket(ticketId, vehicleId);

    const [historyResponse, activeResponse] = await Promise.all([
      apiService.getTicketHistory(1, 100),
      apiService.getActiveTicket(),
    ]);

    setPurchasedTickets(
      historyResponse.items.filter(ticket => !ticket.isValidated)
    );
    setActiveTicket(activeResponse ?? []);

    setShowVehiclePicker(false);
    setSelectedTicketId(null);
    setSelectedVehicleId(null);

    Alert.alert('Sukces', 'Bilet został aktywowany!');
  } catch (error) {
    console.error('Błąd aktywacji biletu:', error);

    Alert.alert(
      'Błąd',
      error instanceof Error
        ? error.message
        : 'Nie udało się aktywować biletu.'
    );
  } finally {
    setIsActivating(false);
  }
};

  return (
    <SafeAreaView style={styles.container} edges={['top', 'left', 'right']}>
      <View style={styles.topBar}>
        <View style={styles.logoContainer}>
          <View style={styles.logoRow}>
            <Image source={Logo} style={styles.logo} resizeMode="contain" />
            <Text style={styles.brandName}>VeloCity</Text>
          </View>
          <Text style={styles.subtitle}>System Zarządzania Transportem</Text>
        </View>

        <TouchableOpacity 
          style={styles.walletHeaderCard}
          onPress={() => navigation.navigate('Main', {screen: 'PaymentHistory', params: { screen: 'Wallet' }})}
        >
          <Text style={styles.walletHeaderLabel}>Portfel 💳</Text>
          {isLoading ? (
            <ActivityIndicator size="small" color="#346699" style={{ marginTop: 2 }} />
          ) : (
            <Text style={styles.walletHeaderBalance}>{`${balance} PLN`}</Text>
          )}
        </TouchableOpacity>
      </View>

      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        
        <View style={styles.welcomeSection}>
          <Text style={styles.greetingText}>Cześć, {user?.name}! 👋</Text>
          <Text style={styles.subtitleText}>Dokąd dzisiaj jedziemy?</Text>
        </View>

        <View style={styles.actionSection}>
          <View style={styles.authContent}>
            <Text style={styles.sectionTitle}>Twoje bilety</Text>
            <Text style={styles.sectionTitle}>
              Bilety do aktywacji
            </Text>

            {purchasedTickets.length > 0 ? (
              purchasedTickets.map((ticket) => (
                <View
                  key={ticket.id}
                  style={styles.activeTicketCard}
                >
                  <Text style={styles.ticketLine}>
                    {ticket.ticketTypeName}
                  </Text>

                  <Text style={styles.ticketTime}>
                    Status: Nieaktywowany
                  </Text>

                  <Text style={styles.ticketTime}>
                    Kupiono: {new Date(ticket.purchasedAt).toLocaleString('pl-PL')}
                  </Text>

                  <Text style={styles.ticketTime}>
                    Cena: {ticket.price.toFixed(2)} zł
                  </Text>

                  <TouchableOpacity
                    style={styles.primaryButton}
                    onPress={() => {
                      setSelectedTicketId(ticket.id);
                      setSelectedVehicleId(null);
                      setShowVehiclePicker(true);
                    }}
                  >
                    <Text style={styles.buttonText}>
                      Aktywuj bilet
                    </Text>
                  </TouchableOpacity>
                </View>
              ))
            ) : (
              <Text style={styles.noTicketText}>
                Nie masz biletów oczekujących na aktywację.
              </Text>
            )}
            
            <Text style={styles.sectionTitle}>Twoje aktywne bilety</Text>
            {activeTicket.length > 0 ? (
              activeTicket.map((ticket) => (
                <View
                  key={ticket.id}
                  style={[styles.activeTicketCard, styles.hasTicketCard]}
                >
                  <Text style={styles.ticketLine}>
                    {ticket.ticketTypeName}
                  </Text>

                  {ticket.vehicleId !== null && ticket.vehicleId !== undefined ? (
                    <Text style={styles.ticketType}>
                      Pojazd: #{ticket.vehicleId}
                    </Text>
                  ) : (
                    <Text style={styles.ticketType}>
                      Bilet sieciowy
                    </Text>
                  )}

                  {ticket.isValidated ? (
                    <View>
                      <Text style={styles.ticketTime}>
                        Status: Bilet aktywowany
                      </Text>

                      <Text style={styles.ticketTime}>
                        {ticket.validTo
                          ? `Ważny do: ${new Date(ticket.validTo).toLocaleString('pl-PL')}`
                          : 'Brak określonej daty końcowej'}
                      </Text>
                    </View>
                  ) : (
                    <View>
                      <Text style={styles.ticketTime}>
                        Status: Bilet nieaktywny
                      </Text>

                      <TouchableOpacity
                        style={styles.primaryButton}
                        onPress={() =>
                          navigation.navigate('ValidateTicket', {
                            ticketId: ticket.id,
                          })
                        }
                      >
                        <Text style={styles.buttonText}>
                          Aktywuj bilet
                        </Text>
                      </TouchableOpacity>
                    </View>
                  )}

                  <View style={styles.qrContainer}>
                    <Text style={styles.qrPlaceholder}>
                      {ticket.isValidated
                        ? '📷 Bilet skasowany'
                        : '⚠️ Wymaga skasowania'}
                    </Text>

                    {ticket.isValidated && (
                      <Text style={styles.ticketIdText}>
                        ID: Velo-{ticket.id}
                      </Text>
                    )}
                  </View>

                  <View style={{ alignItems: 'center', marginTop: 20, marginBottom: 20 }}>
                    <Text
                      style={{
                        fontSize: 16,
                        fontWeight: 'bold',
                        marginBottom: 12,
                        color: '#1A1C1E',
                      }}
                    >
                      Kod QR biletu
                    </Text>

                    <QRCode
                      value={`velocity:ticket:${ticket.id}`}
                      size={180}
                      backgroundColor="white"
                      color="black"
                    />

                    <Text style={{ marginTop: 10, fontSize: 12, color: '#666' }}>
                      Pokaż kod kierowcy podczas kontroli
                    </Text>
                  </View>
                </View>
              ))
            ) : (
              <View style={styles.activeTicketCard}>
                <Text style={styles.noTicketText}>
                  Brak aktywnych biletów w tej chwili.
                </Text>
              </View>
            )}

            <TouchableOpacity 
              style={styles.primaryButton}
              onPress={() => navigation.navigate('PurchaseTicket')}
            >
              <Text style={styles.buttonText}>🎫 Kup nowy bilet</Text>
            </TouchableOpacity>
          </View>
        </View>

      </ScrollView>
      <Modal
        visible={showVehiclePicker}
        transparent
        animationType="slide"
        onRequestClose={() => setShowVehiclePicker(false)}
      >
  <View style={styles.modalOverlay}>
    <View style={styles.modalContent}>
      <Text style={styles.sectionTitle}>
        Wybierz pojazd
      </Text>

      <Text style={styles.ticketTime}>
        Wybierz pojazd, w którym chcesz aktywować bilet.
      </Text>

      <ScrollView style={{ maxHeight: 300 }}>
        {vehicles.length > 0 ? (
          vehicles.map((vehicle) => (
            <TouchableOpacity
              key={vehicle.id}
              style={[
                styles.vehicleOption,
                selectedVehicleId === vehicle.id &&
                  styles.selectedVehicle,
              ]}
              onPress={() => setSelectedVehicleId(vehicle.id)}
            >
              <Text>
                Pojazd #{vehicle.id}
              </Text>
            </TouchableOpacity>
          ))
        ) : (
          <Text>Brak dostępnych pojazdów.</Text>
        )}
      </ScrollView>

      <TouchableOpacity
        style={[
          styles.primaryButton,
          (selectedVehicleId === null || isActivating) && { opacity: 0.5 },
        ]}
        disabled={selectedVehicleId === null || isActivating}
        onPress={handleActivateTicket}
      >
        {isActivating ? (
          <ActivityIndicator color="#FFF" />
        ) : (
          <Text style={styles.buttonText}>
            Potwierdź wybór
          </Text>
        )}
      </TouchableOpacity>

      <TouchableOpacity
        onPress={() => setShowVehiclePicker(false)}
        style={{ marginTop: 16 }}
      >
        <Text style={{ textAlign: 'center' }}>
          Anuluj
        </Text>
      </TouchableOpacity>
    </View>
  </View>
</Modal>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F5F7FA',
  },
  scrollContent: {
    flexGrow: 1,
    paddingHorizontal: 20,
    justifyContent: 'center',
    paddingBottom: 60,
  },
  topBar: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 20,
    paddingTop: 10,
    paddingBottom: 10,
  },
  logoContainer: {
    flexDirection: 'column',
    alignItems: 'baseline',
  },
  logoRow: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  logo: {
    width: 30,
    height: 30,
    marginRight: 8,
  },
  brandName: {
    fontSize: 22,
    fontWeight: '900',
    color: '#1A1C1E',
    letterSpacing: -0.5,
  },
  subtitle: {
    fontSize: 10,
    color: '#666',
    fontWeight: '500',
    marginTop: 2,
  },
  walletHeaderCard: {
    backgroundColor: '#FFF',
    paddingVertical: 8,
    paddingHorizontal: 14,
    borderRadius: 12,
    alignItems: 'flex-end',
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 3,
  },
  walletHeaderLabel: {
    fontSize: 11,
    color: '#666',
    fontWeight: '500',
  },
  walletHeaderBalance: {
    fontSize: 14,
    fontWeight: 'bold',
    color: '#346699',
    marginTop: 2,
  },
  welcomeSection: {
    alignItems: 'center',
    marginBottom: 40,
    marginTop: -40, 
  },
  greetingText: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#1A1C1E',
    textAlign: 'center',
  },
  subtitleText: {
    fontSize: 16,
    color: '#666',
    marginTop: 6,
    textAlign: 'center',
  },
  actionSection: {
    width: '100%',
  },
  authContent: {
    width: '100%',
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#1A1C1E',
    marginBottom: 12,
  },
  activeTicketCard: {
    backgroundColor: '#FFF',
    borderRadius: 16,
    padding: 24,
    borderStyle: 'dashed',
    borderWidth: 1.5,
    borderColor: '#346699',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 20,
  },
  hasTicketCard: {
    borderStyle: 'solid',
    borderColor: '#2ECC71',
    backgroundColor: '#EAFAF1',
  },
  noTicketText: {
    color: '#666',
    fontSize: 14,
    textAlign: 'center',
  },
  ticketLine: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#27AE60',
    textAlign: 'center',
  },
  ticketType: {
    fontSize: 14,
    color: '#2C3E50',
    marginTop: 6,
    fontWeight: '500',
  },
  ticketTime: {
    fontSize: 13,
    color: '#7F8C8D',
    marginTop: 4,
  },
  qrPlaceholder: {
    marginTop: 14,
    fontWeight: 'bold',
    color: '#27AE60',
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  qrContainer: {
    alignItems: 'center',
    marginTop: 14,
    gap: 4,
  },
  ticketIdText: {
    fontSize: 10,
    color: '#7F8C8D',
    fontFamily: 'monospace',
  },
  primaryButton: {
    backgroundColor: '#346699',
    width: '100%',
    padding: 16,
    borderRadius: 14,
    alignItems: 'center',
    justifyContent: 'center',
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  buttonText: {
    color: '#FFF',
    fontSize: 16,
    fontWeight: 'bold',
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    justifyContent: 'center',
    padding: 20,
  },
  modalContent: {
    backgroundColor: '#FFF',
    borderRadius: 16,
    padding: 24,
  },
  vehicleOption: {
    padding: 16,
    borderWidth: 1,
    borderColor: '#DDD',
    borderRadius: 10,
    marginVertical: 5,
  },
  selectedVehicle: {
    borderColor: '#346699',
    backgroundColor: '#EAF2FA',
  },
});

export default PassengerDashboard;
