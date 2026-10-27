import React, { useState, useEffect } from 'react';
import Logo from '../assets/logo.png';
import { View, Text, StyleSheet, TouchableOpacity, ScrollView, Image, ActivityIndicator } from 'react-native';
import { useIsFocused } from '@react-navigation/native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useAuthStore } from '../store/useAuthStore';
import apiService from '../api/apiService';
import { TicketDto } from '../types/ticket';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RootStackParamList } from '../navigation/types';

interface PassengerDashboardProps {
  navigation: NativeStackNavigationProp<RootStackParamList>; 
}

const PassengerDashboard = ({ navigation }: PassengerDashboardProps) => {
  const { user } = useAuthStore();
  const [balance, setBalance] = useState<string>('0.00');
  const [activeTicket, setActiveTicket] = useState<TicketDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);

  const isFocused = useIsFocused();

  useEffect(() => {
    const fetchDashboardData = async () => {
      setIsLoading(true);
      try {
        const walletResponse = await apiService.getBalance();
        setBalance(Number(walletResponse.balance).toFixed(2));

        const ticketResponse = await apiService.getActiveTicket();
        if (ticketResponse && ticketResponse.length > 0)
          setActiveTicket(ticketResponse ?? []);
        
      } catch (error) {
        console.error('Błąd podczas pobierania danych pulpitu:', error);
        setActiveTicket([]);
      } finally {
        setIsLoading(false);
      }
    };

    if(isFocused) fetchDashboardData();
  }, [isFocused]);

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
            <Text style={styles.sectionTitle}>Twój aktywne bilety</Text>
            
            
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

                  <Text style={styles.ticketTime}>
                    {ticket.validTo
                      ? `Ważny do: ${new Date(ticket.validTo).toLocaleString('pl-PL')}`
                      : ticket.isValidated
                        ? 'Bilet aktywowany — bez określonej daty końcowej'
                        : 'Ważność: Czeka na skasowanie'}
                  </Text>

                  <View style={styles.qrContainer}>
                    <Text style={styles.qrPlaceholder}>
                      {ticket.isValidated
                        ? '📷 Bilet skasowany'
                        : '⚠️ Wymaga skasowania'}
                    </Text>

                    <Text style={styles.ticketIdText}>
                      ID: Velo-{ticket.id}
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
});

export default PassengerDashboard;
