import React from 'react';
import AlertMessage from './AlertMessage';

const NewProcessForm = ({ handleSubmit, handleInputChange, handleFileChange, showAlert, showMain }: any) => {
    return (
        <div className="component-container">
            {showAlert && <AlertMessage message="¡El archivo se cargó exitosamente!" />}
            {!showAlert && (
                <div>
                    <div className="divTittle">
                        <h1>Nuevo Proceso de selección</h1>
                    </div>
                    <h3>Por favor ingrese el nombre de la acción y el archivo</h3>
                    <form onSubmit={handleSubmit} encType="multipart/form-data">
                        <div>
                            <input type="text" id="name" onChange={handleInputChange} className="file-input" required placeholder="Nombre" />
                        </div>
                        <div>
                            <input type="file" id="file" onChange={handleFileChange} className="file-input" required />
                        </div>
                        <div>
                            <button type="submit">Enviar</button>
                        </div>
                    </form>
                </div>
            )}
            <div>
                <button onClick={showMain}>Listado de aplicaciones</button>
            </div>
        </div>
    );
}

export default NewProcessForm;
