#include <metahost.h>
#include <string>
#include <wrl\client.h>
#include <Shlwapi.h>

#pragma comment(lib, "mscoree.lib")
#pragma comment(lib, "Shlwapi.lib")

#import "mscorlib.tlb" raw_interfaces_only high_property_prefixes("_get" ,"_put", "_putref") rename("ReportEvent", "InteropServices_ReportEvent")

using namespace std;
using namespace mscorlib;
using namespace Microsoft::WRL;

__declspec(dllexport) HRESULT BlackBitsInjection(LPUTSTR lpArguments)
{
	wchar_t buffer[MAX_PATH];
	StrCpyW(buffer, lpArguments);

	PathAppend(buffer, L"BBitsCore.exe");

	HRESULT hr = E_FAIL;
	ComPtr<ICLRMetaHost> pMetaHost;

	if (FAILED(hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost))))
		return hr;

	ComPtr<ICLRRuntimeInfo> pRuntimeInfo;
	if (FAILED(hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo))))
		return hr;

	ComPtr<ICLRRuntimeHost> pClrRuntimeHost;
	if (FAILED(hr = pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost))))
		return hr;

	if (FAILED(hr = pClrRuntimeHost->Start()))
		return hr;

	DWORD pReturnValue;
	if (FAILED(hr = pClrRuntimeHost->ExecuteInDefaultAppDomain(buffer, L"BBitsCore.Program", L"BlackBitsInitialize", L"", &pReturnValue)))
		return hr;

	if (pReturnValue == 0)
	{
		// All good
	}
	else if (pReturnValue == 1)
	{
		//Already Running
		MessageBoxW(nullptr, L"BlackBits is already running on this process.", L"BlackBits", 0);
	}
	else
	{
		// Error
		
	}

	return hr;
}
/*
wchar_t* GetBootstrapPath()
{
	// get full path to exe
	wchar_t buffer[MAX_PATH];
	GetModuleFileName(NULL, buffer, MAX_PATH);
	
	// remove filename
	PathRemoveFileSpecW(buffer);

	// append bootstrap to buffer if it will fit
	size_t size = wcslen(buffer) + wcslen(L"BBitsCore.dll");
	if (size <= MAX_PATH)
		PathAppend(buffer, L"BBitsCore.dll");
	else
		throw runtime_error("Module name cannot exceed MAX_PATH");
	
	// return path
	return buffer;
}*/

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	return TRUE;
}