import React, { useState, ChangeEvent, FormEvent } from 'react';
import axios from 'axios';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const UploadCvForm = ({ detailResponse, showMain }: any) => {
    const [file, setFile] = useState<File | null>(null);

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
            toast.success('¡CV subido con éxito!');
            // Refrescar la lista de CVs después de subir uno nuevo
            // fetchReceivedCvs();
        } catch (error) {
            console.error('Error al subir el CV:', error);
            toast.error('Error al subir el CV');
        }
    };

    return (
        <div className="component-container">
            <ToastContainer />
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
                        required
                        disabled
                    />
                </div>
                <div className="div-flex">
                    <label className="field-input-label">Descripción del cargo</label>
                    <textarea
                        className='input-textarea'
                        value={detailResponse.data.jobDescription}
                        required
                        disabled
                        rows={12}
                        cols={150}
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

export default UploadCvForm;
