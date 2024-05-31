import React from 'react';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import AlertMessage from './AlertMessage';

const NewProcessForm = ({ handleSubmit, handleApplicationNameChange, handleApplicationJobDescriptionChange, handleFileChange, showAlert, showMain }: any) => {
    const onSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        try {
            await handleSubmit(event);
            toast.success('¡El archivo se cargó exitosamente!');
        } catch (error) {
            toast.error('Error al cargar el archivo');
        }
    };

    return (
        <div className="component-container">
            <ToastContainer />
            {showAlert && <AlertMessage message="¡El archivo se cargó exitosamente!" />}
            {!showAlert && (
                <div>
                    <div className="divTittle">
                        <h1>Nuevo Proceso de Selección</h1>
                    </div>
                    <form onSubmit={onSubmit} encType="multipart/form-data">
                        <div className="div-flex">
                            <label className="field-input-label">Nombre del Proceso:</label>
                            <input
                                type="text"
                                id="name"
                                name="name"
                                onChange={handleApplicationNameChange}
                                required
                                placeholder="Nombre del Proceso"
                            />
                        </div>
                        <br />
                        <div className="div-flex">
                            <label className="field-input-label">Descripción del Puesto:</label>
                            <textarea
                                className='input-textarea'
                                id="jobDescription"
                                name="jobDescription"
                                onChange={handleApplicationJobDescriptionChange}
                                required
                                placeholder="Descripción del Puesto"
                                rows={12} // Ajusta el número de filas según sea necesario
                                cols={100}
                            />
                        </div>
                        <div className="div-flex">
                            <label className="field-input-label">Archivo de Candidatos:</label>
                            <input
                                type="file"
                                id="file"
                                name="file"
                                onChange={handleFileChange}
                                required
                            />
                        </div>
                        <div className="form-group">
                            <button type="submit" className="btn btn-primary">Enviar</button>
                        </div>
                    </form>
                </div>
            )}
            <div>
                <button onClick={showMain} className="btn btn-secondary">Volver</button>
            </div>
        </div>
    );
}

export default NewProcessForm;
