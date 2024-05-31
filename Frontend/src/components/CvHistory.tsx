import React, { useState, useEffect } from 'react';
import axios from 'axios';

interface ReceivedCv {
    receivedCvId: string;
    candidateName: string;
    candidateEmail: string;
    calification: number;
    explanation: string;
    processingDate: string;
}

const CvHistory = ({ detailResponse, showMain }: any) => {
    const [receivedCvs, setReceivedCvs] = useState<ReceivedCv[]>([]);

    useEffect(() => {
        const fetchReceivedCvs = async () => {
            try {
                const response = await axios.get(process.env.REACT_APP_BACKEND_URI! + '/api/history', {
                    params: {
                        applicationId: detailResponse.data.id,
                        page: 1,
                        pageSize: 10
                    }
                });
                setReceivedCvs(response.data.data);
            } catch (error) {
                console.error('Error al obtener el historial de CVs:', error);
            }
        };

        fetchReceivedCvs();
    }, [detailResponse.data.id]);

    return (
        <div className="component-container">

<div className="divTittle">
                <h1>Detalles de la acción</h1>
            </div>
            <div className="field">
                <span className="field-label">ID:</span>
                <span className="field-value">{detailResponse.data.id}</span>
            </div>
            <div className="field">
                <span className="field-label">Nombre:</span>
                <span className="field-value">{detailResponse.data.name}</span>
            </div>
            <div className="field">
                <span className="field-label">Estado:</span>
                <span className="field-value">{detailResponse.data.status}</span>
            </div>
            <div className="field">
                <span className="field-label">Fecha de creación:</span>
                <span className="field-value">{detailResponse.data.createdAt}</span>
            </div>
            <div className="field">
                <span className="field-label">URL Base Excel:</span>
                <a className="field-link" href={detailResponse.data.excelUrl} target='_blank'>ver</a>
            </div>
            <div className='field'>
                <span className="field-label">Descripción del cargo</span>
                <span className="field-value">{detailResponse.data.jobDescription}</span>
            </div>

            <div className="divTittle">
                <h2>Historial de CVs Procesados</h2>
            </div>
            <div className="received-cvs">
                {receivedCvs.length === 0 ? (
                    <p>No hay CVs procesados.</p>
                ) : (
                    <table>
                        <thead>
                            <tr>
                                <th>Fecha de Procesamiento</th>
                                <th>Nombre del Candidato</th>
                                <th>Email del Candidato</th>
                                <th>Calificación</th>
                                <th>Explicación</th>
                            </tr>
                        </thead>
                        <tbody>
                            {receivedCvs.map((cv: ReceivedCv) => (
                                <tr key={cv.receivedCvId}>
                                    <td>{new Date(cv.processingDate).toLocaleDateString()}</td>
                                    <td>{cv.candidateName}</td>
                                    <td>{cv.candidateEmail}</td>
                                    <td>{cv.calification}</td>
                                    <td>{cv.explanation}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>
            <div>
                <button onClick={showMain}>Volver</button>
            </div>
        </div>
    );
}

export default CvHistory;
