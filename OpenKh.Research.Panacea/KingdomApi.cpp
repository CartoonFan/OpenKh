#include "KingdomApi.h"
#include "Panacea.h"

#define PFN_DEFINE(NAME) PFN_##NAME pfn_##NAME = nullptr

PFN_DEFINE(Axa_CFileMan_LoadFile);
PFN_DEFINE(Axa_CFileMan_LoadFileWithMalloc);
PFN_DEFINE(Axa_CFileMan_GetFileSize);
PFN_DEFINE(Axa_AxaResourceMan_SetResourceItem);
PFN_DEFINE(Axa_PackageMan_GetFileInfo);
PFN_DEFINE(Axa_CalcHash);
PFN_DEFINE(Axa_SetReplacePath);
PFN_DEFINE(Axa_FreeAllPackages);
PFN_DEFINE(Axa_CFileMan_GetRemasteredCount);
PFN_DEFINE(Axa_CFileMan_GetRemasteredEntry);
PFN_DEFINE(Axa_PackageFile_GetRemasteredAsset);
PFN_DEFINE(Axa_AxaSoundStream__threadProc);
PFN_DEFINE(Axa_OpenFile);
PFN_DEFINE(Axa_DebugPrint);
PFN_DEFINE(Bbs_File_load);
PFN_DEFINE(Bbs_CRsrcData_loadCallback);

long Axa::CFileMan::LoadFile(CFileMan* _this, const char* filename, void* addr, bool unk) {
    return pfn_Axa_CFileMan_LoadFile(_this, filename, addr, unk);
}

void* Axa::CFileMan::LoadFileWithMalloc(CFileMan* _this, const char* filename, int* sizePtr, bool unk, const char* filename2) {
    return pfn_Axa_CFileMan_LoadFileWithMalloc(_this, filename, sizePtr, unk, filename2);
}

long Axa::CFileMan::GetFileSize(CFileMan* _this, const char* filename) {
    return pfn_Axa_CFileMan_GetFileSize(_this, filename);
}

long Axa::AxaResourceMan::SetResourceItem(const char* filename, int size, void* buffer) {
    return pfn_Axa_AxaResourceMan_SetResourceItem(filename, size, buffer);
}

Axa::PackageFile* Axa::PackageMan::GetFileInfo(const char* filename, const char* filename2) {
    return pfn_Axa_PackageMan_GetFileInfo(filename, filename2);
}

__int64 Axa::CalcHash(const void* data, int size, void* dst)
{
    return pfn_Axa_CalcHash(data, size, dst);
}

int Axa::SetReplacePath(__int64 a1, const char* a2)
{
    return pfn_Axa_SetReplacePath(a1, a2);
}

void Axa::FreeAllPackages()
{
    pfn_Axa_FreeAllPackages();
}

__int64 Axa::CFileMan::GetRemasteredCount()
{
    return pfn_Axa_CFileMan_GetRemasteredCount();
}

Axa::RemasteredEntry* Axa::CFileMan::GetRemasteredEntry(CFileMan* a1, int* origOffsetPtr, int assetNum)
{
    return pfn_Axa_CFileMan_GetRemasteredEntry(a1, origOffsetPtr, assetNum);
}

void* Axa::PackageFile::GetRemasteredAsset(Axa::PackageFile* a1, unsigned int* assetSizePtr, int assetNum)
{
    return pfn_Axa_PackageFile_GetRemasteredAsset(a1, assetSizePtr, assetNum);
}

__int64 Axa::AxaSoundStream::_threadProc(unsigned int* instance)
{
    return pfn_Axa_AxaSoundStream__threadProc(instance);
}

int Axa::OpenFile(const char* Format, int OFlag)
{
    return pfn_Axa_OpenFile(Format, OFlag);
}

size_t Bbs::File::load(const char* filename, long long a2) {
    return pfn_Bbs_File_load(filename, a2);
}

void Bbs::CRsrcData::loadCallback(unsigned int* pMem, size_t size, unsigned int* pArg, int nOpt) {
    pfn_Bbs_CRsrcData_loadCallback(pMem, size, pArg, nOpt);
}

#define PVAR_DEFINE(TYPE, NAME) VarPtr<TYPE> NAME
#define PARR_DEFINE(TYPE, NAME, LEN) ArrayPtr<TYPE,LEN> NAME

PVAR_DEFINE(int, PackageFileCount);
PVAR_DEFINE(int, LastOpenedPackage);
PARR_DEFINE(Axa::PackageFile*, PackageFiles, 16);
PARR_DEFINE(char, BasePath, 128);
