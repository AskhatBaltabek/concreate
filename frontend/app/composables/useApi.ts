export const useApi = () => {
  const runtimeConfig = useRuntimeConfig()
  const router = useRouter()
  // baseURL can be taken from config, but let's stick to the current one for now
  const apiBase = 'http://localhost:5239' 

  const fetchWithAuth = async (url: string, options: any = {}) => {
    const token = localStorage.getItem('token')
    
    if (!token && !url.includes('/login') && !url.includes('/register')) {
      router.push('/login')
      return null
    }

    const headers = {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
      ...options.headers
    }

    try {
      const response = await fetch(`${apiBase}${url}`, {
        ...options,
        headers
      })

      if (response.status === 401) {
        localStorage.removeItem('token')
        router.push('/login')
        return null
      }

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(errorText || `HTTP error! status: ${response.status}`)
      }

      // Handle empty response (like DELETE 204)
      if (response.status === 204) return true

      return await response.json()
    } catch (error) {
      console.error('API Error:', error)
      throw error
    }
  }

  return {
    fetchWithAuth,
    apiBase
  }
}
