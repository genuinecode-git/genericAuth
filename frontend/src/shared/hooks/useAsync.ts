import { useState, useCallback, useRef, useEffect } from 'react';
import { AsyncState, AsyncStatus } from '../types';

/**
 * Custom hook for managing async operations
 * Handles loading, success, and error states automatically
 */
export function useAsync<T = unknown, E = Error>(
  initialState?: Partial<AsyncState<T, E>>
) {
  const [state, setState] = useState<AsyncState<T, E>>({
    status: 'idle',
    data: undefined,
    error: undefined,
    ...initialState,
  });

  const isMountedRef = useRef(true);

  useEffect(() => {
    return () => {
      isMountedRef.current = false;
    };
  }, []);

  const setData = useCallback((data: T) => {
    if (isMountedRef.current) {
      setState({
        status: 'success',
        data,
        error: undefined,
      });
    }
  }, []);

  const setError = useCallback((error: E) => {
    if (isMountedRef.current) {
      setState({
        status: 'error',
        data: undefined,
        error,
      });
    }
  }, []);

  const setPending = useCallback(() => {
    if (isMountedRef.current) {
      setState((prev) => ({
        ...prev,
        status: 'pending',
        error: undefined,
      }));
    }
  }, []);

  const reset = useCallback(() => {
    if (isMountedRef.current) {
      setState({
        status: 'idle',
        data: undefined,
        error: undefined,
      });
    }
  }, []);

  const execute = useCallback(
    async (asyncFunction: () => Promise<T>): Promise<T | undefined> => {
      setPending();
      try {
        const data = await asyncFunction();
        setData(data);
        return data;
      } catch (error) {
        setError(error as E);
        return undefined;
      }
    },
    [setPending, setData, setError]
  );

  return {
    ...state,
    isIdle: state.status === 'idle',
    isPending: state.status === 'pending',
    isSuccess: state.status === 'success',
    isError: state.status === 'error',
    setData,
    setError,
    setPending,
    reset,
    execute,
  };
}

export type UseAsyncReturn<T, E = Error> = ReturnType<typeof useAsync<T, E>>;
