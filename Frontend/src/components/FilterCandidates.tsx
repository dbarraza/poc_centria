import React, { useState, ChangeEvent, FormEvent } from 'react';

const FilterCandidates = ({ detailResponse, showMain }: any) => {
    const [filters, setFilters] = useState({
        minSalaryExpect: "",
        maxSalaryExpect: "",
        policeRecord: "",
        criminalRecord: "",
        judicialRecord: "",
        consent: "",
        hasFamiliar: "",
        query: ""
    });

    const handleInputChange = (event: ChangeEvent<HTMLInputElement>) => {
        const { name, value } = event.target;
        setFilters(prevState => ({ ...prevState, [name]: value }));
    };

    const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        // Aquí iría la lógica para filtrar los candidatos usando los filtros
        console.log(filters);
    };

    return (
        <div className="component-container">
            <div className="divTittle">
                <h1>Filtrado de candidatos</h1>
            </div>
            <div className="field">
                <span className="field-label">ID:</span>
                <span className="field-value">{detailResponse.data.id}</span>
            </div>
            <div className="field">
                <span className="field-label">Nombre:</span>
                <span className="field-value">{detailResponse.data.name}</span>
            </div>
            <form onSubmit={handleSubmit}>
                <div>
                    <input type="text" name="minSalaryExpect" value={filters.minSalaryExpect} onChange={handleInputChange} placeholder="Minimum Salary Expectation" required />
                </div>
                <div>
                    <input type="text" name="maxSalaryExpect" value={filters.maxSalaryExpect} onChange={handleInputChange} placeholder="Maximum Salary Expectation" required />
                </div>
                <div>
                    <input type="text" name="policeRecord" value={filters.policeRecord} onChange={handleInputChange} placeholder="Police Record" required />
                </div>
                <div>
                    <input type="text" name="criminalRecord" value={filters.criminalRecord} onChange={handleInputChange} placeholder="Criminal Record" required />
                </div>
                <div>
                    <input type="text" name="judicialRecord" value={filters.judicialRecord} onChange={handleInputChange} placeholder="Judicial Record" required />
                </div>
                <div>
                    <input type="text" name="consent" value={filters.consent} onChange={handleInputChange} placeholder="Consent" required />
                </div>
                <div>
                    <input type="text" name="hasFamiliar" value={filters.hasFamiliar} onChange={handleInputChange} placeholder="Has Familiar" required />
                </div>
                <div>
                    <input type="text" name="query" value={filters.query} onChange={handleInputChange} placeholder="Query" required />
                </div>
                <div>
                    <button type="submit">Filtrar</button>
                </div>
            </form>
            <div>
                <button onClick={showMain}>Procesos de selección</button>
            </div>
        </div>
    );
}

export default FilterCandidates;
