import React, { useState, ChangeEvent, FormEvent } from 'react';
import axios from 'axios';
import AlertMessage from './AlertMessage';

const CvProcessing = ({ detailResponse, showMain }: any) => {
    const [file, setFile] = useState<File | null>(null);
    const [showSuccessAlert, setShowSuccessAlert] = useState(false);

    const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
        if (event.target.files && event.target.files.length > 0) {
            setFile(event.target.files[0]);
        }
    };

    const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        const formData = new FormData();
        if (file) {
            formData.append('file', file);
        }
        formData.append('applicationId', detailResponse.data.id);

        try {
            const response = await axios.post(process.env.REACT_APP_BACKEND_URI! + '/api/uploadCv', formData, {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            });
            console.log('CV subido con éxito:', response.data);
            setShowSuccessAlert(true);
            setTimeout(() => setShowSuccessAlert(false), 3000); // Oculta el mensaje después de 3 segundos
        } catch (error) {
            console.error('Error al subir el CV:', error);
        }
    };

    return (
        <div className="component-container">
            {showSuccessAlert && <AlertMessage message="¡CV subido con éxito!" />}
            <div className="divTittle">
                <h1>Carga de Cvs</h1>
            </div>
            <form onSubmit={handleSubmit}>
                <div className="div-flex">
                    <label className="field-input-label">ID:</label>
                    <input
                        type="text"
                        name="id"
                        value={detailResponse.data.id}
                        placeholder="ID"
                        required
                        disabled
                    />
                </div>
                <div className="div-flex">
                    <label className="field-input-label">Nombre del proceso:</label>
                    <input
                        type="text"
                        name="name"
                        value={detailResponse.data.name}
                        placeholder="Name"
                        required
                        disabled
                    />
                </div>
                <div className="div-flex">
                    <label className="field-input-label">Cargar CV:</label>
                    <input
                        type="file"
                        name="file"
                        onChange={handleFileChange}
                        required
                    />
                </div>
                <div className="div-flex">
                    <button type="submit">Enviar</button>
                </div>
            </form>
            <div>
                <button onClick={showMain}>Volver</button>
            </div>
        </div>
    );
}

export default CvProcessing;
