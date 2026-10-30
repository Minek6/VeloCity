import React, { useEffect, useState, useRef } from 'react';
import { View, Text, StyleSheet, SafeAreaView, ScrollView, TouchableOpacity, Alert, Pressable } from 'react-native';
import { useAuthStore } from '../store/useAuthStore';
import apiService from '../api/apiService';
import { Vehicle } from '../types';
import { useCameraPermission} from 'react-native-vision-camera';
import { Camera as CameraKitCamera } from 'react-native-camera-kit';

const DriverDashboard = ({ navigation }: any) => {
  const { user } = useAuthStore();
  const [isDriving, setIsDriving] = useState(false);
  const { hasPermission, requestPermission } = useCameraPermission();
  const [isScannerOpen, setIsScannerOpen] = useState(false);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [selectedVehicleId, setSelectedVehicleId] = useState<number | null>(null);
  const [vehicleDropdownOpen, setVehicleDropdownOpen] = useState(false);

  const isProcessingQr = useRef(false);

  useEffect(() => {
    const fetchVehicles = async () => {
      try {
        const response = await apiService.getVehicles();
        console.log('POJAZDY Z API:', JSON.stringify(response, null, 2));
        setVehicles(response ?? []);
      } catch (error) {
        console.error('Błąd pobierania pojazdów:', error);
        Alert.alert('Błąd', 'Nie udało się pobrać listy pojazdów.');
      }
    };

    fetchVehicles();
  }, []);

    // Funkcja przełączająca stan kursu
    const handleToggleRoute = () => {
      setIsDriving(!isDriving);
    };

  const handleVerifyTicket = async () => {
    try {
      if (!hasPermission) {
        const granted = await requestPermission();

        if (!granted) {
          Alert.alert(
            'Brak dostępu do aparatu',
            'Zezwól aplikacji na korzystanie z aparatu.',
          );
          return;
        }
      }

      setIsScannerOpen(true);
    } catch (error) {
      console.error('Błąd uruchamiania aparatu:', error);
      Alert.alert('Błąd', 'Nie udało się uruchomić aparatu.');
    }
  };

  const selectedVehicle = vehicles.find(
    vehicle => vehicle.id === selectedVehicleId,
  );

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView contentContainerStyle={styles.padding} showsVerticalScrollIndicator={false}>
        
        
        {/* --- NAGŁÓWEK --- */}
        <View style={styles.header}>
          <Text style={styles.roleBadge}>KIEROWCA 🚌</Text>
          <Text style={styles.welcomeText}>Cześć, {user?.name || 'Kierowco'}</Text>
        </View>

        {/* --- STATUS ZMIANY --- */}
        <View style={styles.statusBanner}>
          <View style={[styles.statusDot, { backgroundColor: isDriving ? '#2ECC71' : '#F1C40F' }]} />
          <Text style={styles.statusText}>
            Status: {isDriving ? 'W TRASIE (KURS AKTYWNY)' : 'POSTÓJ / OCZEKIWANIE'}
          </Text>
        </View>

        <View style={{marginVertical: 16}}>
          <Text style={{fontSize: 18, fontWeight: 'bold', marginBottom: 12}}>
            Wybierz pojazd
          </Text>

          <Pressable
            disabled={isDriving}
            onPress={() => setVehicleDropdownOpen(!vehicleDropdownOpen)}
            style={{
              padding: 14,
              borderRadius: 10,
              borderWidth: 1,
              borderColor: '#cccccc',
              backgroundColor: '#ffffff',
              flexDirection: 'row',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}>
            <Text style={{color: '#111827', fontSize: 16}}>
              {selectedVehicleId !== null
                ? (() => {
                    const vehicle = vehicles.find(
                      v => v.id === selectedVehicleId,
                    );

                    return vehicle
                      ? `${vehicle.sideNumber} — ${vehicle.model}`
                      : 'Wybierz pojazd';
                  })()
                : 'Wybierz pojazd'}
            </Text>

            <Text style={{color: '#111827'}}>
              {vehicleDropdownOpen ? '▲' : '▼'}
            </Text>
          </Pressable>

          {vehicleDropdownOpen && !isDriving && (
            <ScrollView
              nestedScrollEnabled
              style={{
                maxHeight: 240,
                borderWidth: 1,
                borderColor: '#cccccc',
                borderRadius: 10,
                backgroundColor: '#ffffff',
                marginTop: 4,
              }}>
              {vehicles
                .filter(vehicle => vehicle.isActive)
                .map(vehicle => (
                  <Pressable
                    key={vehicle.id}
                    onPress={() => {
                      setSelectedVehicleId(vehicle.id);
                      setVehicleDropdownOpen(false);
                    }}
                    style={{
                      padding: 14,
                      borderBottomWidth: 1,
                      borderBottomColor: '#eeeeee',
                      backgroundColor:
                        selectedVehicleId === vehicle.id
                          ? '#dbeafe'
                          : '#ffffff',
                    }}>
                    <Text style={{color: '#111827', fontSize: 15}}>
                      {vehicle.sideNumber} — {vehicle.model}
                    </Text>
                  </Pressable>
                ))}
            </ScrollView>
          )}
        </View>

        {/* --- KARTA BIEŻĄCEGO KORSU --- */}
        <View style={styles.routeCard}>
          <Text style={styles.cardLabel}>PRZYPISANY KURS</Text>

          <Text style={styles.lineText}>Linia 502</Text>

          <View style={styles.routeRow}>
            <Text style={styles.stationText}>Centrum</Text>
            <Text style={styles.arrowText}>➔</Text>
            <Text style={styles.stationText}>Plac Centralny</Text>
          </View>

          <View style={styles.divider} />

          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Planowany odjazd:</Text>
            <Text style={styles.infoValue}>16:45</Text>
          </View>

          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Pojazd:</Text>
            <Text style={styles.infoValue}>
              {selectedVehicle
                ? `${selectedVehicle.sideNumber} — ${selectedVehicle.model}`
                : 'Nie wybrano pojazdu'}
            </Text>
          </View>
        </View>

        {/* --- GŁÓWNY PRZYCISK AKCJI --- */}
        <TouchableOpacity 
          style={[styles.actionButton, isDriving ? styles.buttonStop : styles.buttonStart]} 
          onPress={handleToggleRoute}
        >
          <Text style={styles.buttonText}>
            {isDriving ? '🛑 Zakończ bieżący kurs' : '▶ Rozpocznij kurs'}
          </Text>          
        </TouchableOpacity>
        
        {/* --- SZYBKIE INFORMACJE O ZMIANIE --- */}

        {/* --- KONTROLA BILETÓW --- */}
        <View style={styles.ticketVerificationCard}>
          <Text style={styles.cardLabel}>
            KONTROLA BILETÓW
          </Text>

          <Text style={styles.verificationTitle}>
            Weryfikacja biletu
          </Text>

          <Text style={styles.verificationDescription}>
            Zeskanuj kod QR pasażera, aby sprawdzić
            ważność biletu.
          </Text>

          <TouchableOpacity
            style={styles.verifyButton}
            onPress={handleVerifyTicket}
          >
            <Text style={styles.buttonText}>
              📷 Sprawdź bilet
            </Text>
          </TouchableOpacity>
        </View>

        {isScannerOpen && (
          <View style={styles.scannerContainer}>
            <Text style={styles.verificationTitle}>
              Skaner biletów QR
            </Text>

            <CameraKitCamera
              style={styles.cameraPreview}
              
              scanBarcode={true}
              onReadCode={(event) => {
                const qrValue = event.nativeEvent.codeStringValue;

                if (!qrValue || isProcessingQr.current) {
                  return;
                }

                isProcessingQr.current = true;
                setIsScannerOpen(false);

                Alert.alert(
                  'Zeskanowano kod QR',
                  `Odczytana wartość: ${qrValue}`,
                  [
                    {
                      text: 'OK',
                      onPress: () => {
                        isProcessingQr.current = false;
                      },
                    },
                  ],
                );
              }}
            />

            <TouchableOpacity
              style={styles.closeScannerButton}
              onPress={() => setIsScannerOpen(false)}
            >
              <Text style={styles.buttonText}>
                Zamknij aparat
              </Text>
            </TouchableOpacity>
          </View>
        )}

        <View style={styles.shiftSummary}>
          <Text style={styles.summaryTitle}>Podsumowanie dzisiejszej zmiany</Text>
          <View style={styles.summaryRow}>
            <Text style={styles.summaryLabel}>Czas pracy:</Text>
            <Text style={styles.summaryValue}>3h 45m / 8h</Text>
          </View>
          <View style={styles.summaryRow}>
            <Text style={styles.summaryLabel}>Zrealizowane kursy:</Text>
            <Text style={styles.summaryValue}>4 / 9</Text>
          </View>
        </View>

      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { 
    flex: 1, 
    backgroundColor: '#F5F7FA' 
  },
  padding: { 
    padding: 20 
  },
  header: { 
    marginBottom: 16, 
    marginTop: 10 
  },
  roleBadge: { 
    color: '#E67E22', 
    fontWeight: 'bold', 
    fontSize: 12, 
    letterSpacing: 1 
  },
  welcomeText: { 
    fontSize: 26, 
    fontWeight: 'bold', 
    color: '#1A1C1E',
    marginTop: 4
  },
  statusBanner: {
    backgroundColor: '#FFF',
    borderRadius: 10,
    padding: 12,
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 20,
    borderWidth: 1,
    borderColor: '#E0E0E0',
  },
  statusDot: {
    width: 10,
    height: 10,
    borderRadius: 5,
    marginRight: 10,
  },
  statusText: {
    fontSize: 13,
    fontWeight: 'bold',
    color: '#1A1C1E',
  },
  routeCard: {
    backgroundColor: '#FFF',
    borderRadius: 16,
    padding: 20,
    elevation: 3,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.05,
    shadowRadius: 6,
    marginBottom: 24,
  },
  cardLabel: {
    fontSize: 11,
    color: '#8E9297',
    fontWeight: 'bold',
    letterSpacing: 0.5,
  },
  lineText: {
    fontSize: 32,
    fontWeight: '900',
    color: '#346699',
    marginTop: 4,
  },
  routeRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: 10,
    marginBottom: 16,
  },
  stationText: {
    fontSize: 16,
    fontWeight: '600',
    color: '#1A1C1E',
  },
  arrowText: {
    marginHorizontal: 10,
    color: '#8E9297',
    fontSize: 16,
  },
  divider: {
    height: 1,
    backgroundColor: '#EEF0F2',
    marginBottom: 16,
  },
  infoRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 8,
  },
  infoLabel: {
    color: '#666',
    fontSize: 14,
  },
  infoValue: {
    fontWeight: '600',
    color: '#1A1C1E',
    fontSize: 14,
  },
  actionButton: {
    width: '100%',
    padding: 18,
    borderRadius: 14,
    alignItems: 'center',
    justifyContent: 'center',
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    marginBottom: 24,
  },
  buttonStart: {
    backgroundColor: '#2ECC71',
  },
  buttonStop: {
    backgroundColor: '#E74C3C',
  },
  buttonText: {
    color: '#FFF',
    fontSize: 18,
    fontWeight: 'bold',
  },
  shiftSummary: {
    backgroundColor: '#EAEDF1',
    borderRadius: 12,
    padding: 16,
  },
  summaryTitle: {
    fontSize: 14,
    fontWeight: 'bold',
    color: '#1A1C1E',
    marginBottom: 12,
  },
  summaryRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 6,
  },
  summaryLabel: {
    fontSize: 13,
    color: '#555',
  },
  summaryValue: {
    fontSize: 13,
    fontWeight: 'bold',
    color: '#1A1C1E',
  },
  ticketVerificationCard: {
    backgroundColor: '#FFF',
    borderRadius: 16,
    padding: 20,
    marginBottom: 24,
    elevation: 3,
  },
  verificationTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#1A1C1E',
    marginTop: 8,
    marginBottom: 8,
  },
  verificationDescription: {
    fontSize: 14,
    color: '#666',
    lineHeight: 21,
    marginBottom: 16,
  },
  verifyButton: {
    backgroundColor: '#346699',
    padding: 16,
    borderRadius: 12,
    alignItems: 'center',
  },
  scannerContainer: {
  backgroundColor: '#FFF',
  borderRadius: 16,
  padding: 16,
  marginBottom: 24,
  },
  cameraPreview: {
    width: '100%',
    height: 350,
    borderRadius: 12,
    overflow: 'hidden',
  },
  closeScannerButton: {
    backgroundColor: '#E74C3C',
    padding: 16,
    borderRadius: 12,
    alignItems: 'center',
    marginTop: 16,
  },
});

export default DriverDashboard;