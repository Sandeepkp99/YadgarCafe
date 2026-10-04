const base=import.meta.env.VITE_API_BASE_URL||'/api';
export const token=()=>localStorage.getItem('yadgar.access');
export async function api(path,options={}){const r=await fetch(base+path,{...options,headers:{'Content-Type':'application/json',...(token()?{Authorization:`Bearer ${token()}`}:{ }),...options.headers}});if(r.status===204)return null;const b=await r.json().catch(()=>null);if(!r.ok)throw Error(b?.message||b?.title||Object.values(b?.errors||{}).flat().join(' ')||`Request failed (${r.status})`);return b}
export function saveSession(r){if(r?.data?.accessToken){localStorage.setItem('yadgar.access',r.data.accessToken);localStorage.setItem('yadgar.refresh',r.data.refreshToken);localStorage.setItem('yadgar.user',JSON.stringify(r.data.user))}}
export function clearSession(){['yadgar.access','yadgar.refresh','yadgar.user'].forEach(k=>localStorage.removeItem(k))}



