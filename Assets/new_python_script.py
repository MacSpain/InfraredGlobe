import cdsapi
import zipfile
import os
import numpy as np
from netCDF4 import Dataset


def download_data_from_cds(dataset_name, query, output_zip):
    c = cdsapi.Client()

    c.retrieve(
        dataset_name,
        query,
        output_zip
    )
    print(f'Plik {output_zip} został pobrany.')

def unzip_file(zip_path, extract_to):
    with zipfile.ZipFile(zip_path, 'r') as zip_ref:
        zip_ref.extractall(extract_to)
    print(f'Plik został rozpakowany do katalogu: {extract_to}')

def read_nc_file(extract, nc_file):
    i = 1
    for file in nc_file:
        dataset = Dataset(os.path.join(extract, file), 'r')
        print(f'Odczytano plik NetCDF: {os.path.join(extract, file)}')
    
        print("Zmienne w pliku:")
        for var in dataset.variables:
            print(var)

        keylist = list(dataset.variables.keys())
        for key in keylist:
            example_var = key 
            data = dataset.variables[example_var][:] 
            print(f'Przykładowe dane z zmiennej {example_var}:', data)
            if isinstance(data.data, np.ndarray):
                data.data.astype('float').tofile('./Assets/Data/extracted_data/data/' + example_var + '.dat')


        dataset.close()

if __name__ == "__main__":
    
    extract_to = './Assets/Data/extracted_data'
    
    nc_files = [f for f in os.listdir(extract_to) if f.endswith('.nc')]
    if nc_files:
        read_nc_file(extract_to, nc_files)
    else:
        print("Nie znaleziono plików .nc w rozpakowanym folderze.")