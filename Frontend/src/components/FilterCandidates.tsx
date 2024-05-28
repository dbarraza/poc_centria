import React, { useState, ChangeEvent, FormEvent } from 'react';
import axios from 'axios';

const FilterCandidates = ({ detailResponse, showMain }: any) => {
    const [filters, setFilters] = useState({
        minSalaryExpect: "4000", // Valor por defecto
        maxSalaryExpect: "7000", // Valor por defecto
        policeRecord: "No",
        criminalRecord: "No",
        judicialRecord: "No",
        consent: "Sí, estoy de acuerdo y otorgo mi consentimiento",
        hasFamiliar: "No, ningún tipo de relación",
        query: ""
    });

    const [results, setResults] = useState<any[]>([]); // Inicializar como un array vacío

    const [showResult, setShowResult] = useState(false); // Inicializar como falso

    const handleInputChange = (event: ChangeEvent<HTMLInputElement>) => {
        const { name, value } = event.target;
        setFilters(prevState => ({ ...prevState, [name]: value }));
    };

    const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setShowResult(false); // Cambia el estado a true para indicar que la solicitud está en curso
        try {
            const response = await axios.get(process.env.REACT_APP_BACKEND_URI! + `/api/application/${detailResponse.data.id}/prefiltered-candidates`, {
                params: filters
            });

            // Verificar que response.data.data sea un array
            if (Array.isArray(response.data.data)) {
                setResults(response.data.data);
            } else {
                alert('La respuesta de la API no es un array:' + response.data.data);
            }
        } catch (error) {
            console.error('Error al enviar la solicitud:', error);
        }
        finally {
            setShowResult(true); // Independientemente del resultado de la solicitud, establece el estado a false para indicar que la solicitud ha terminado
        }
    };


    return (
        <div className="component-container">
            <div className="divTittle">
                <h1>Filtrado de candidatos</h1>
            </div>
            <form onSubmit={handleSubmit}>
                <div className="div-flex">
                    <span className="field-input-label">ID:</span>
                    <input
                        className='.field-input-text'
                        type="text"
                        name="id"
                        value={detailResponse.data.id}
                        onChange={handleInputChange}
                        placeholder="ID"
                        required
                        disabled={true}
                    />
                </div>
                <div className="div-flex">
                    <span className="field-input-label">Nombre del proceso:</span>
                    <input
                        className='.field-input-text'
                        type="text"
                        name="name"
                        value={detailResponse.data.name}
                        onChange={handleInputChange}
                        placeholder="Name"
                        required
                        disabled={true}
                    />
                </div>
                <div className="div-flex">
                    <span className="field-input-label">Salario Mínimo:</span>
                    <input
                        className='.field-input-text'
                        type="text"
                        name="minSalaryExpect"
                        value={filters.minSalaryExpect}
                        onChange={handleInputChange}
                        placeholder="Minimum Salary Expectation"
                    />
                </div>
                <div className="div-flex">
                    <label className="field-input-label">Salario Máximo:</label>
                    <input
                        type="text"
                        name="maxSalaryExpect"
                        value={filters.maxSalaryExpect}
                        onChange={handleInputChange}
                        placeholder="Maximum Salary Expectation"
                    />
                </div>
                <div className="div-flex">
                    <label className="field-input-label">Antecedentes Policiales:</label>
                    <input
                        type="text"
                        name="policeRecord"
                        value={filters.policeRecord}
                        onChange={handleInputChange}
                        placeholder="Police Record"
                    />
                </div>
                <div className="div-flex">
                    <label className="field-input-label">Antecedentes Criminales:</label>
                    <input
                        type="text"
                        name="criminalRecord"
                        value={filters.criminalRecord}
                        onChange={handleInputChange}
                        placeholder="Criminal Record"
                    />
                </div>
                <div className="div-flex">
                    <label className="field-input-label">Antecedentes Judiciales:</label>
                    <input
                        type="text"
                        name="judicialRecord"
                        value={filters.judicialRecord}
                        onChange={handleInputChange}
                        placeholder="Judicial Record"
                    />
                </div>
                <div className="div-flex">
                    <label className="field-input-label">Consentimiento: </label>
                    <input
                        type="text"
                        name="consent"
                        value={filters.consent}
                        onChange={handleInputChange}
                        placeholder="Consent"
                    />
                </div>
                <div className="div-flex">
                    <label className="field-input-label">Familiar en la Empresa:</label>
                    <input
                        type="text"
                        name="hasFamiliar"
                        value={filters.hasFamiliar}
                        onChange={handleInputChange}
                        placeholder="Has Familiar"
                    />
                </div>
                <div className="div-flex">
                    <label className="field-input-label">Otra Consulta:</label>
                    <input
                        type="text"
                        name="query"
                        value={filters.query}
                        onChange={handleInputChange}
                        placeholder="Acá puede agregar otro tipo de consulta"
                    />
                </div>
                <br />
                <div>
                    <button type="submit">Filtrar</button>
                </div>
            </form>

            {/* Resultados */}
            {showResult && results.length > 0 && (
            <div>
                <h2>Resultados</h2>
                <table>
                    <thead>
                        <tr>
                            <th>Id</th>
                            <th>Nombre</th>
                            <th>Email</th>
                            <th>Disponibilidad</th>
                            <th>Expectativa Salarial</th>
                            <th>Antecendetes Policiales</th>
                            <th>Antecendetes Criminales</th>
                            <th>Antecendetes Judiciales</th>
                            <th>Familiar en la Empresa</th>
                            <th>Resultado</th>
                        </tr>
                    </thead>
                    <tbody>
                        {results.map((candidate: any, index: number) => (
                            <tr key={index}>
                                <td>{candidate.candidateId}</td>
                                <td>{candidate.name}</td>
                                <td>{candidate.email}</td>
                                <td>{candidate.availabilityForWork}</td>
                                <td>{candidate.salaryExpectation}</td>
                                <td>{candidate.policeRecord}</td>
                                <td>{candidate.criminalRecord}</td>
                                <td>{candidate.judicialRecord}</td>
                                <td>{candidate.hasFamiliar}</td>
                                <td>{candidate.highlights}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
            )}

            <div>
                <button onClick={showMain}>Procesos de selección</button>
            </div>
        </div>

        // <div className="component-container">
            
        // </div>
    );
}

export default FilterCandidates;
