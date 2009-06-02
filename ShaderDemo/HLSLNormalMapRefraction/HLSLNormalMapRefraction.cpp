//////////////////////////////////////////////////////////////////////////////////////////////////
// File: HLSLNormalMapRefraction.cpp
// Author: Chris Smith
// Date Created: 3/22/06
// Description: Renders a mesh with a normal map refracted texture projected onto it (HLSL shader)
// Copyright: Use this however you want, but I am not responsible for anything
//
// Controls: W,A,S,D or mouse buttons to move, mouse rotates camera, ESC to quit
//////////////////////////////////////////////////////////////////////////////////////////////////
#define STRICT
#define WIN32_LEAN_AND_MEAN

float ScreenWidth  = 640;
float ScreenHeight = 480;

#include <windows.h>
#include <mmsystem.h>
#include <d3d9.h>
#include <d3dx9.h>
#include "DirectInput.h"
#include "Camera.h"

// Globals
HWND              hwnd      = NULL;
IDirect3DDevice9* D3DDevice = NULL;

DirectInput *DInput;
Camera *ActiveCamera;
ID3DXMesh* RoomMesh;
ID3DXMesh* BallMesh;
ID3DXMesh* BillboardMesh;
ID3DXMesh* BorderMesh;
IDirect3DTexture9* RoomTexture;
IDirect3DTexture9* BallTexture;
IDirect3DTexture9* ProjectedTexture;
IDirect3DTexture9* NormalMapTexture;
IDirect3DSurface9* RenderSurface;
ID3DXEffect* RefractionEffect;

D3DXMATRIX Proj;

DWORD CurrentTime;
DWORD LastTime;
float DeltaTime;

// Prototypes
void UpdateDeltaTime(void);
void LoadXFile( char* MeshFilename, ID3DXMesh* &Mesh );
void Setup(void);
void Cleanup(void);
void RenderScene(void);
void RenderBillboard(void);
void RenderSceneToTexture(void);
void Render(void);
void InitializeDirect3D(void);
LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow);

///////////////////////////////////////////////////////
// GetDeltaTime
///////////////////////////////////////////////////////
void UpdateDeltaTime()
{
	CurrentTime = timeGetTime();

	DeltaTime = ((float)CurrentTime - (float)LastTime) * 0.001f;

	LastTime = timeGetTime();
}

///////////////////////////////////////////////////////
// LoadXFile
///////////////////////////////////////////////////////
void LoadXFile( char* MeshFilename, ID3DXMesh* &Mesh )
{
	//Zero Mesh and create buffer
	Mesh = 0;
	ID3DXBuffer* MeshBuffer  = 0;

	//Load and optimize the mesh
	D3DXLoadMeshFromX( MeshFilename, D3DXMESH_MANAGED, D3DDevice, &MeshBuffer, 0, 0, 0, &Mesh);
	Mesh->OptimizeInplace( D3DXMESHOPT_ATTRSORT | D3DXMESHOPT_COMPACT | D3DXMESHOPT_VERTEXCACHE, (DWORD*)MeshBuffer->GetBufferPointer(), 0, 0, 0);

	//Release and zero the buffer
	MeshBuffer->Release(); MeshBuffer = NULL;
}

///////////////////////////////////////////////////////
// Setup
///////////////////////////////////////////////////////
void Setup( void )
{
	//Load the X file into RoomMesh
	LoadXFile("Room.X", RoomMesh);

	//Load the X file into BorderMesh
	LoadXFile("Border.X", BorderMesh);

	//Load the X file into BillboardMesh
	LoadXFile("BillBoard.X", BillboardMesh);

	//Load the X file into BallMesh
	LoadXFile("Ball.X", BallMesh);

	//Load the texture into RoomTexture
	D3DXCreateTextureFromFile( D3DDevice, "conc02.jpg", &RoomTexture );

	//Load the texture into BallTexture
	D3DXCreateTextureFromFile( D3DDevice, "stone_wall.bmp", &BallTexture );

	//Load the texture into NormalMapTexture
	D3DXCreateTextureFromFile( D3DDevice, "NormalMap.bmp", &NormalMapTexture );

	//Create the ProjectedTexture object
	D3DDevice->CreateTexture(512, 512, 1, D3DUSAGE_RENDERTARGET, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &ProjectedTexture, NULL);
	ProjectedTexture->GetSurfaceLevel(0, &RenderSurface);

	//Load the HLSL shader
	D3DXCreateEffectFromFile( D3DDevice, "HLSLNormalMapRefraction.fx", NULL, NULL, NULL, NULL, &RefractionEffect, NULL );

	//Create a new instance of Camera
	ActiveCamera = new Camera();

	//Set the Projection matrix
	D3DXMatrixPerspectiveFovLH( &Proj, D3DX_PI/4.0f, ScreenWidth / ScreenHeight, 1.0f, 1000.0f );
	D3DDevice->SetTransform(D3DTS_PROJECTION, &Proj);

	//Disable lighting for this sample
	D3DDevice->SetRenderState( D3DRS_LIGHTING, false );

	//Set some render states to determine texture quality
	D3DDevice->SetSamplerState(0, D3DSAMP_MAGFILTER, D3DTEXF_LINEAR);
	D3DDevice->SetSamplerState(0, D3DSAMP_MINFILTER, D3DTEXF_LINEAR);
	D3DDevice->SetSamplerState(0, D3DSAMP_MIPFILTER, D3DTEXF_LINEAR);
}

///////////////////////////////////////////////////////
// Cleanup
///////////////////////////////////////////////////////
void Cleanup( void )
{
	//Release the RoomMesh object
	RoomMesh->Release();

	//Release the BorderMesh object
	BorderMesh->Release();

	//Release the BillboardMesh object
	BillboardMesh->Release();

	//Release the BallMesh object
	BallMesh->Release();

	//Release the RoomTexture object
	RoomTexture->Release();

	//Release the BallTexture object
	BallTexture->Release();

	//Release the ProjectedTexture object
	ProjectedTexture->Release();

	//Release the NormalMapTexture object
	NormalMapTexture->Release();

	//Release the back render targets
	RenderSurface->Release();

	//Release the PointLight effect
	RefractionEffect->Release();

	//Delete the Direct Input device
	delete DInput;

	//Delete the Camera object
	delete ActiveCamera;

	//Release the Direct3D device
	D3DDevice->Release();
}

///////////////////////////////////////////////////////
// Render the scene
///////////////////////////////////////////////////////
void RenderScene()
{
	//Set the world matrix
	D3DXMATRIX World;
	D3DXMatrixIdentity(&World);
	D3DDevice->SetTransform(D3DTS_WORLD, &World);

	//Render the room
	D3DDevice->SetTexture(0, RoomTexture);
	RoomMesh->DrawSubset(0);

	//Render the ball
	World._42 = 6.0f;
	World._43 = 20.0f;
	D3DDevice->SetTransform(D3DTS_WORLD, &World);
	D3DDevice->SetTexture(0, BallTexture);
	BallMesh->DrawSubset(0);

	//Render the glass's border
	World._43 = 0.0f;
	D3DDevice->SetTransform(D3DTS_WORLD, &World);
	BorderMesh->DrawSubset(0);
}

///////////////////////////////////////////////////////
// Render the normal map refracted billboard
///////////////////////////////////////////////////////
void RenderBillboard()
{
	//Set the world matrix
	D3DXMATRIX World;
	D3DXMatrixIdentity(&World);
	D3DDevice->SetTransform(D3DTS_WORLD, &World);

	//Move the world position up for the billboard
	World._42 = 6.0f;

	//Set the texture adjustment matrix offsets
	float XOffset = 0.5f + (0.5f / (float)512); //Width of the light texture
	float YOffset = 0.5f + (0.5f / (float)512); //Height of the light texture

	//Create the texture adjustment matrix
	D3DXMATRIX TexMatrix;
	ZeroMemory(&TexMatrix, sizeof(D3DXMATRIX));
	TexMatrix._11 = 0.5f;
	TexMatrix._22 = -0.5f;
	TexMatrix._33 = 0.5f;
	TexMatrix._41 = XOffset;
	TexMatrix._42 = YOffset;
	TexMatrix._43 = 0.5f;
	TexMatrix._44 = 1.0f;	

	//Set dynamic shader variables
	RefractionEffect->SetTechnique( "NormalMapRefraction" );
	RefractionEffect->SetMatrix( "WorldViewProj", &(World * ActiveCamera->View * Proj) );
	RefractionEffect->SetMatrix( "TexTransform", &(World * ActiveCamera->View * Proj * TexMatrix) );
	RefractionEffect->SetTexture( "NormalTex", NormalMapTexture );
	RefractionEffect->SetTexture( "ProjTex", ProjectedTexture );

	//Begin the shader pass
	UINT Pass, Passes;
	RefractionEffect->Begin(&Passes, 0);
	for (Pass = 0; Pass < Passes; Pass++)
	{
		RefractionEffect->BeginPass(Pass);

		//Render the Room
		BillboardMesh->DrawSubset(0);

		RefractionEffect->EndPass();
	}
	RefractionEffect->End();
}

///////////////////////////////////////////////////////
// Render the scene to the texture
///////////////////////////////////////////////////////
void RenderSceneToTexture()
{
	//Get the old back buffer
	IDirect3DSurface9* BackBuffer;
	D3DDevice->GetRenderTarget(0, &BackBuffer);

	//Set the proj textures render surface
	D3DDevice->SetRenderTarget(0, RenderSurface);

	//Render the scene
	RenderScene();

	//Set the render target to the old back buffer
	D3DDevice->SetRenderTarget(0, BackBuffer);
	BackBuffer->Release();

	//Clear the backbuffer
	D3DDevice->Clear( 0, NULL, D3DCLEAR_TARGET | D3DCLEAR_ZBUFFER, 0x00000000, 1.0f, 0 );
}

///////////////////////////////////////////////////////
// Render
///////////////////////////////////////////////////////
void Render( void )
{
	//Update the Delta Time
	UpdateDeltaTime();

	//Poll the DirectInput device
	DInput->Poll();

	//Move Camera
	float ForwardUnits = 0;
	float SidewardUnits = 0;

	//Process keyboard presses
	if( DInput->KeyDown( DIK_W ) )
		ForwardUnits = 50.0f * DeltaTime;
	if( DInput->KeyDown( DIK_S ) )
		ForwardUnits = -50.0f * DeltaTime;
	if( DInput->KeyDown( DIK_A ) )
		SidewardUnits = -50.0f * DeltaTime;
	if( DInput->KeyDown( DIK_D ) )
		SidewardUnits = 50.0f * DeltaTime; 

	//Process mouse button presses
	if( DInput->MouseButtonDown(0) )
		ForwardUnits = 50.0f * DeltaTime;
	if( DInput->MouseButtonDown(1) )
		ForwardUnits = -50.0f * DeltaTime;

	//Rotate Camera
	ActiveCamera->Pitch -= (double)DInput->MouseState.lY * DeltaTime * 0.1f;
	ActiveCamera->Yaw -= (double)DInput->MouseState.lX * DeltaTime * 0.1f;

	//Update the camera
	ActiveCamera->UpdateCamera(ForwardUnits, SidewardUnits);

	//Update the active view
	D3DDevice->SetTransform(D3DTS_VIEW, &ActiveCamera->View);

    D3DDevice->Clear( 0, NULL, D3DCLEAR_TARGET | D3DCLEAR_ZBUFFER, 0x00000000, 1.0f, 0 );
    D3DDevice->BeginScene();

	//Render the scene to the texture
	RenderSceneToTexture();

	//Render the normal map refracted billboard
	RenderBillboard();

	//Render the room to the normal back buffer
	RenderScene();

    D3DDevice->EndScene();
    D3DDevice->Present( NULL, NULL, NULL, NULL );
}

///////////////////////////////////////////////////////
// WindowProc
///////////////////////////////////////////////////////
LRESULT CALLBACK WindowProc( HWND hWnd, UINT msg,  WPARAM wParam, LPARAM lParam )
{
    switch( msg )
	{
        case WM_KEYDOWN:
		{
			if( wParam == VK_ESCAPE )
			{
				PostQuitMessage(0);
				break;
			}
		}
        break;

		case WM_CLOSE:
		{
			PostQuitMessage(0);
		}
		
        case WM_DESTROY:
		{
            PostQuitMessage(0);
		}
        break;

		default:
		{
			return DefWindowProc( hWnd, msg, wParam, lParam );
		}
		break;
	}
	return 0;
}

///////////////////////////////////////////////////////
// InitializeDirect3D
///////////////////////////////////////////////////////
void InitializeDirect3D( void )
{
	IDirect3D9* D3D = NULL;

    D3D = Direct3DCreate9( D3D_SDK_VERSION );

	if( !D3D )
	{
		if( D3D != NULL )
			D3D->Release();

		::MessageBox(0, "Direct3DCreate9() - Failed", 0, 0);
		return;
	}

    D3DDISPLAYMODE d3ddm;

    if( FAILED( D3D->GetAdapterDisplayMode( D3DADAPTER_DEFAULT, &d3ddm ) ) )
	{
		if( D3D != NULL )
			D3D->Release();

		::MessageBox(0, "GetAdapterDisplayMode() - Failed", 0, 0);
		return;
	}

	HRESULT hr;

	if( FAILED( hr = D3D->CheckDeviceFormat( D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, 
												d3ddm.Format, D3DUSAGE_DEPTHSTENCIL,
												D3DRTYPE_SURFACE, D3DFMT_D16 ) ) )
	{
		if( hr == D3DERR_NOTAVAILABLE )
		{
			if( D3D != NULL )
				D3D->Release();

			::MessageBox(0, "CheckDeviceFormat() - Failed", 0, 0);
			return;
		}
	}

	D3DCAPS9 d3dCaps;

	if( FAILED( D3D->GetDeviceCaps( D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, &d3dCaps ) ) )
	{
		if( D3D != NULL )
			D3D->Release();

		::MessageBox(0, "GetDeviceCaps() - Failed", 0, 0);
		return;
	}

	DWORD dwBehaviorFlags = 0;

	// Use hardware vertex processing if supported, otherwise default to software 
	if( d3dCaps.VertexProcessingCaps != 0 )
		dwBehaviorFlags |= D3DCREATE_HARDWARE_VERTEXPROCESSING;
	else
		dwBehaviorFlags |= D3DCREATE_SOFTWARE_VERTEXPROCESSING;

	// All system checks passed, create the device

	D3DPRESENT_PARAMETERS d3dpp;
	memset(&d3dpp, 0, sizeof(d3dpp));

    d3dpp.BackBufferFormat       = d3ddm.Format;
	d3dpp.SwapEffect             = D3DSWAPEFFECT_DISCARD;
	d3dpp.Windowed               = TRUE;
    d3dpp.EnableAutoDepthStencil = TRUE;
	d3dpp.BackBufferHeight       = ScreenHeight;
	d3dpp.BackBufferWidth        = ScreenWidth;
    d3dpp.AutoDepthStencilFormat = D3DFMT_D16;
    d3dpp.PresentationInterval   = D3DPRESENT_INTERVAL_ONE;

    if( FAILED( D3D->CreateDevice( D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, hwnd,
                                      dwBehaviorFlags, &d3dpp, &D3DDevice ) ) )
	{
		if( D3D != NULL )
			D3D->Release();

		::MessageBox(0, "CreateDevice() - Failed", 0, 0);
		return;
	}

	// No longer needed, release it
	 if( D3D != NULL )
        D3D->Release();
}

///////////////////////////////////////////////////////
// WinMain
///////////////////////////////////////////////////////
int WINAPI WinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow )
{
	WNDCLASSEX winClass;
	MSG        uMsg;

    memset(&uMsg,0,sizeof(uMsg));

	winClass.lpszClassName = "MainWindow";
	winClass.cbSize        = sizeof(WNDCLASSEX);
	winClass.style         = CS_HREDRAW | CS_VREDRAW;
	winClass.lpfnWndProc   = WindowProc;
	winClass.hInstance     = hInstance;
	winClass.hIcon	       = ::LoadIcon(0, IDI_APPLICATION);
    winClass.hIconSm	   = ::LoadIcon(0, IDI_APPLICATION);
	winClass.hCursor       = LoadCursor(NULL, IDC_ARROW);
	winClass.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH);
	winClass.lpszMenuName  = NULL;
	winClass.cbClsExtra    = 0;
	winClass.cbWndExtra    = 0;

	if( !RegisterClassEx(&winClass) )
		return E_FAIL;

	hwnd = CreateWindowEx( NULL, "MainWindow", 
                             "HLSL Normal Map Refraction - by Chris Smith",
						     WS_OVERLAPPEDWINDOW,
					         0, 0, ScreenWidth, ScreenHeight, NULL, NULL, hInstance, NULL );

	if( hwnd == NULL )
		return E_FAIL;

    ShowWindow( hwnd, nCmdShow );
    UpdateWindow( hwnd );

	InitializeDirect3D();

	//Initialize the Direct Input object
	DInput = new DirectInput();
	DInput->InitializeDirectInput(hInstance, hwnd, DISCL_NONEXCLUSIVE|DISCL_FOREGROUND,
												   DISCL_EXCLUSIVE|DISCL_FOREGROUND);

	Setup();

	while( uMsg.message != WM_QUIT )
	{
		if( PeekMessage( &uMsg, NULL, 0, 0, PM_REMOVE ) )
		{ 
			TranslateMessage( &uMsg );
			DispatchMessage( &uMsg );
		}
        else
		    Render();
	}

	Cleanup();

    UnregisterClass( "MY_WINDOWS_CLASS", winClass.hInstance );

	return uMsg.wParam;
}