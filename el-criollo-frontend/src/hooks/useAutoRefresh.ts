import { useState, useEffect, useCallback, useRef } from 'react';

interface UseAutoRefreshOptions {
  enabled?: boolean;
  interval?: number;
  onRefresh: () => void | Promise<void>;
}

interface UseAutoRefreshReturn {
  isEnabled: boolean;
  isRefreshing: boolean;
  lastRefresh: Date | null;
  toggleAutoRefresh: () => void;
  enableAutoRefresh: () => void;
  disableAutoRefresh: () => void;
  refreshNow: () => Promise<void>;
  setInterval: (interval: number) => void;
}

export const useAutoRefresh = (options: UseAutoRefreshOptions): UseAutoRefreshReturn => {
  const { enabled = true, interval = 30000, onRefresh } = options;

  const [isEnabled, setIsEnabled] = useState(enabled);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [lastRefresh, setLastRefresh] = useState<Date | null>(null);
  const [currentInterval, setCurrentInterval] = useState(interval);
  const intervalRef = useRef<number | null>(null);

  const refreshNow = useCallback(async () => {
    try {
      setIsRefreshing(true);
      await onRefresh();
      setLastRefresh(new Date());
    } catch (error) {
      console.error('Error during refresh:', error);
    } finally {
      setIsRefreshing(false);
    }
  }, [onRefresh]);

  const startAutoRefresh = useCallback(() => {
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }

    if (isEnabled) {
      intervalRef.current = window.setInterval(refreshNow, currentInterval);
    }
  }, [isEnabled, currentInterval, refreshNow]);

  const stopAutoRefresh = useCallback(() => {
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
  }, []);

  const toggleAutoRefresh = useCallback(() => {
    setIsEnabled((prev) => !prev);
  }, []);

  const enableAutoRefresh = useCallback(() => {
    setIsEnabled(true);
  }, []);

  const disableAutoRefresh = useCallback(() => {
    setIsEnabled(false);
  }, []);

  const setInterval = useCallback((newInterval: number) => {
    setCurrentInterval(newInterval);
  }, []);

  // Efecto para manejar el auto-refresh
  useEffect(() => {
    if (isEnabled) {
      startAutoRefresh();
    } else {
      stopAutoRefresh();
    }

    return () => {
      stopAutoRefresh();
    };
  }, [isEnabled, currentInterval, startAutoRefresh, stopAutoRefresh]);

  // Cargar datos iniciales
  useEffect(() => {
    refreshNow();
  }, []);

  return {
    isEnabled,
    isRefreshing,
    lastRefresh,
    toggleAutoRefresh,
    enableAutoRefresh,
    disableAutoRefresh,
    refreshNow,
    setInterval,
  };
};
